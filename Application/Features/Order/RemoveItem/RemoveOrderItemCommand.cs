using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.RemoveItem;

public sealed class RemoveOrderItemCommand : IRequest<OrderResponse>
{
    public Guid OrderId { get; set; }
    public Guid? OrderManagerId { get; set; }
    public Guid ItemId { get; set; }
}
