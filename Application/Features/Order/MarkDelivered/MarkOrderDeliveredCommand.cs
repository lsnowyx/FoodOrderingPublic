using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.MarkDelivered;

public sealed class MarkOrderDeliveredCommand : IRequest<OrderResponse>
{
    public Guid Id { get; set; }
    public Guid? OrderManagerId { get; set; }
}
