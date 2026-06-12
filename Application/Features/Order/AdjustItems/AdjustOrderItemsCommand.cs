using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.AdjustItems;

public sealed class AdjustOrderItemsCommand : IRequest<OrderResponse>
{
    public Guid OrderId { get; set; }
    public Guid? OrderManagerId { get; set; }
    public List<AdjustItemDto> Items { get; set; } = new();
}
