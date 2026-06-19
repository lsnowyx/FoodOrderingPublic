using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.UpdateStatus;

public sealed class UpdateOrderStatusCommand : IRequest<OrderResponse>
{
    public Guid Id { get; set; }
    public Guid OrderManagerId { get; set; }
    public string Status { get; set; } = null!;
}
