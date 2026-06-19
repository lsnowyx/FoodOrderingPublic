using Application.Abstractions.Repositories;
using Application.DTOs.Order;
using Common.Enums;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Order.Create;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IOrdersRepository _ordersRepo;
    private readonly IMenuItemsRepository _menuRepo;
    private readonly UserManager<User> userManager;

    public CreateOrderCommandHandler(IOrdersRepository ordersRepo, IMenuItemsRepository menuRepo, UserManager<User> userManager)
    {
        _ordersRepo = ordersRepo;
        _menuRepo = menuRepo;
        this.userManager = userManager;
    }

    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == null) throw new ArgumentException("UserId is required for registered customer orders");

        var customer = await userManager.FindByIdAsync(request.UserId.Value.ToString());
        if (customer == null) throw new KeyNotFoundException("Customer not found");

        var order = new Domain.Entities.Order
        {
            UserId = request.UserId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            IsPaid = false,
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

        await _ordersRepo.AddAsync(order, cancellationToken);
        await _ordersRepo.SaveChangesAsync(cancellationToken);

        return OrderResponseFactory.Create(order);
    }
}
