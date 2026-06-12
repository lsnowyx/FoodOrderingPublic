using Application.DTOs.Order;
using Domain.Entities;

namespace Application.Features.Order;

internal static class OrderResponseFactory
{
    public static OrderResponse Create(Domain.Entities.Order order)
    {
        return new OrderResponse(
            order.Id,
            order.CustomerId,
            order.AssignedOrderManagerId,
            order.AssignedAt,
            order.OrderDate,
            order.Status.ToString(),
            order.IsPaid,
            order.DeliveryAddress,
            order.DeliveredAt,
            order.UpdatedAt,
            order.OrderItems.Select(oi => new OrderItemDto(
                oi.Id,
                oi.MenuItemId,
                oi.MenuItem?.Name ?? string.Empty,
                oi.Quantity,
                oi.UnitPrice)));
    }
}
