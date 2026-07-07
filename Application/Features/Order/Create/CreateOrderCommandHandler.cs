using Application.Abstractions.Repositories;
using Application.Abstractions.Persistence;
using Application.Abstractions.Services;
using Application.DTOs.Order;
using Common.Enums;
using Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Order.Create;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IOrdersRepository _ordersRepo;
    private readonly IMenuItemsRepository _menuRepo;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IApplicationTransaction _applicationTransaction;
    private readonly UserManager<User> userManager;

    public CreateOrderCommandHandler(
        IOrdersRepository ordersRepo,
        IMenuItemsRepository menuRepo,
        IStripePaymentService stripePaymentService,
        IApplicationTransaction applicationTransaction,
        UserManager<User> userManager)
    {
        _ordersRepo = ordersRepo;
        _menuRepo = menuRepo;
        _stripePaymentService = stripePaymentService;
        _applicationTransaction = applicationTransaction;
        this.userManager = userManager;
    }

    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == null) throw new ArgumentException("UserId is required for registered customer orders");

        var customer = await userManager.FindByIdAsync(request.UserId.Value.ToString());
        if (customer == null) throw new KeyNotFoundException("Customer not found");

        OrderItemQuantityGuard.EnsureValidOrderShape(
            request.Items,
            item => item.Quantity);

        var order = new Domain.Entities.Order
        {
            UserId = request.UserId,
            User = customer,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            IsPaid = false,
            PaymentMethod = request.PayOnline
                ? PaymentMethod.OnlineCard
                : PaymentMethod.CashOnDelivery,
            PaymentStatus = request.PayOnline
                ? PaymentStatus.PendingOnlinePayment
                : PaymentStatus.Unpaid,
            DeliveryAddress = request.DeliveryAddress.Trim(),
            DeliveryLatitude = request.DeliveryLatitude,
            DeliveryLongitude = request.DeliveryLongitude,
            OrderItems = new List<OrderItem>()
        };

        foreach (var it in request.Items)
        {
            var menu = await _menuRepo.GetByIdAsync(it.MenuItemId, cancellationToken);
            if (menu == null) throw new KeyNotFoundException($"MenuItem not found: {it.MenuItemId}");
            if (!menu.IsAvailable)
                throw new ArgumentException($"Menu item is unavailable: {it.MenuItemId}");

            var oi = new OrderItem
            {
                MenuItemId = it.MenuItemId,
                Quantity = it.Quantity,
                UnitPrice = menu.Price
            };

            order.OrderItems.Add(oi);
        }

        _ordersRepo.RecalculateTotalsAsync(order);

        if (!request.PayOnline)
        {
            await _ordersRepo.AddAsync(order, cancellationToken);
            await _ordersRepo.SaveChangesAsync(cancellationToken);

            return order.Adapt<CreateOrderResponse>();
        }

        return await _applicationTransaction.ExecuteAsync(
            async transactionCancellationToken =>
            {
                await _ordersRepo.AddAsync(order, transactionCancellationToken);
                await _ordersRepo.SaveChangesAsync(transactionCancellationToken);

                string paymentUrl;
                try
                {
                    var paymentAttemptResult = await _stripePaymentService.CreateNewPaymentAttemptForOrderAsync(
                        order,
                        transactionCancellationToken);
                    paymentUrl = paymentAttemptResult.PaymentUrl;
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    throw new InvalidOperationException(
                        "Could not start online payment. Please try again.",
                        exception);
                }

                var response = order.Adapt<CreateOrderResponse>();
                return response with { PaymentUrl = paymentUrl };
            },
            cancellationToken);
    }
}
