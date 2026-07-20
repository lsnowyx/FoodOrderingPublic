using Application.Abstractions.Repositories;
using Application.Abstractions.Persistence;
using Application.Abstractions.Services;
using Application.DTOs.Checkout;
using Application.Features.Order;
using Common.Enums;
using Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Checkout.Guest;

public class GuestCheckoutCommandHandler : IRequestHandler<GuestCheckoutCommand, GuestCheckoutResponse>
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IMenuItemsRepository _menuItemsRepository;
    private readonly IOrderTrackingTokenService _trackingTokenService;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IApplicationTransaction _applicationTransaction;
    private readonly IEmailService _emailService;
    private readonly ILogger<GuestCheckoutCommandHandler> _logger;

    public GuestCheckoutCommandHandler(
        IOrdersRepository ordersRepository,
        IMenuItemsRepository menuItemsRepository,
        IOrderTrackingTokenService trackingTokenService,
        IStripePaymentService stripePaymentService,
        IApplicationTransaction applicationTransaction,
        IEmailService emailService,
        ILogger<GuestCheckoutCommandHandler> logger)
    {
        _ordersRepository = ordersRepository;
        _menuItemsRepository = menuItemsRepository;
        _trackingTokenService = trackingTokenService;
        _stripePaymentService = stripePaymentService;
        _applicationTransaction = applicationTransaction;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<GuestCheckoutResponse> Handle(GuestCheckoutCommand request, CancellationToken cancellationToken)
    {
        OrderItemQuantityGuard.EnsureValidOrderShape(
            request.Items,
            item => item.Quantity);

        var now = DateTime.UtcNow;
        var guestCustomer = new GuestCustomer
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            CreatedAt = now
        };

        var order = new Domain.Entities.Order
        {
            UserId = null,
            GuestCustomerId = guestCustomer.Id,
            GuestCustomer = guestCustomer,
            OrderDate = now,
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

        foreach (var item in request.Items)
        {
            var menuItem = await _menuItemsRepository.GetByIdAsync(item.MenuItemId, cancellationToken);
            if (menuItem is null)
            {
                throw new KeyNotFoundException($"Menu item not found: {item.MenuItemId}");
            }

            if (!menuItem.IsAvailable)
            {
                throw new ArgumentException($"Menu item is unavailable: {item.MenuItemId}");
            }

            order.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                MenuItemId = menuItem.Id,
                Quantity = item.Quantity,
                UnitPrice = menuItem.Price
            });
        }

        _ordersRepository.RecalculateTotalsAsync(order);

        var rawTrackingToken = _trackingTokenService.GenerateRawToken();
        var trackingLink = _trackingTokenService.CreateTrackingLink(order, rawTrackingToken);
        order.TrackingLinks.Add(trackingLink);

        var response = order.Adapt<GuestCheckoutResponse>();
        response.TrackingToken = rawTrackingToken;

        if (!request.PayOnline)
        {
            return await _applicationTransaction.ExecuteAsync(
                async transactionCancellationToken =>
                {
                    await _ordersRepository.AddAsync(order, transactionCancellationToken);
                    await _ordersRepository.SaveChangesAsync(transactionCancellationToken);
                    await SendTrackingTokenEmailAsync(
                        order.Id,
                        guestCustomer.Email,
                        rawTrackingToken,
                        transactionCancellationToken);

                    return response;
                },
                cancellationToken);
        }

        response = await _applicationTransaction.ExecuteAsync(
            async transactionCancellationToken =>
            {
                await _ordersRepository.AddAsync(order, transactionCancellationToken);
                await _ordersRepository.SaveChangesAsync(transactionCancellationToken);

                string checkoutSessionId;
                try
                {
                    var paymentAttemptResult = await _stripePaymentService.CreateNewPaymentAttemptForOrderAsync(
                        order,
                        transactionCancellationToken);
                    response.PaymentUrl = paymentAttemptResult.PaymentUrl;
                    checkoutSessionId = paymentAttemptResult.CheckoutSessionId;
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    throw new InvalidOperationException(
                        "Could not start online payment. Please try again.",
                        exception);
                }

                try
                {
                    await SendTrackingTokenEmailAsync(
                        order.Id,
                        guestCustomer.Email,
                        rawTrackingToken,
                        transactionCancellationToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    await _stripePaymentService.ExpireCheckoutSessionAsync(
                        checkoutSessionId,
                        CancellationToken.None);

                    throw;
                }

                return response;
            },
            cancellationToken);

        return response;
    }

    private async Task SendTrackingTokenEmailAsync(
        Guid orderId,
        string email,
        string rawTrackingToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        try
        {
            var publicTrackingUrl = _trackingTokenService.CreatePublicTrackingUrl(rawTrackingToken);

            await _emailService.SendAsync(
                email,
                "Your FoodOrdering order tracking",
                $"Your tracking token is: {rawTrackingToken}"
                    + Environment.NewLine
                    + Environment.NewLine
                    + $"Track your order: {publicTrackingUrl}",
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(
                exception,
                "Failed to send guest checkout tracking token email. OrderId={OrderId}",
                orderId);

            throw new InvalidOperationException(
                "Could not send tracking email. Please try again later.",
                exception);
        }
    }
}
