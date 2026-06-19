using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Checkout;
using Common.Enums;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Features.Checkout.Guest;

public class GuestCheckoutCommandHandler : IRequestHandler<GuestCheckoutCommand, GuestCheckoutResponse>
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IMenuItemsRepository _menuItemsRepository;
    private readonly IOrderTrackingTokenService _trackingTokenService;

    public GuestCheckoutCommandHandler(
        IOrdersRepository ordersRepository,
        IMenuItemsRepository menuItemsRepository,
        IOrderTrackingTokenService trackingTokenService)
    {
        _ordersRepository = ordersRepository;
        _menuItemsRepository = menuItemsRepository;
        _trackingTokenService = trackingTokenService;
    }

    public async Task<GuestCheckoutResponse> Handle(GuestCheckoutCommand request, CancellationToken cancellationToken)
    {
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
            DeliveryAddress = request.DeliveryAddress.Trim(),
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

        var rawTrackingToken = _trackingTokenService.GenerateRawToken();
        var trackingLink = _trackingTokenService.CreateTrackingLink(order, rawTrackingToken);
        order.TrackingLinks.Add(trackingLink);

        await _ordersRepository.AddAsync(order, cancellationToken);
        await _ordersRepository.SaveChangesAsync(cancellationToken);

        var response = order.Adapt<GuestCheckoutResponse>();
        response.TrackingToken = rawTrackingToken;

        return response;
    }
}
