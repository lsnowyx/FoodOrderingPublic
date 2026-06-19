using Application.DTOs.Order;
using Domain.Entities;
using Mapster;

namespace Application.Features.Order;

internal static class OrderResponseFactory
{
    public static OrderResponse Create(Domain.Entities.Order order)
    {
        return order.Adapt<OrderResponse>();
    }
}
