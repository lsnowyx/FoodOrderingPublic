using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.Take;

public sealed class TakeOrderCommand : IRequest<OrderResponse>
{
    public Guid OrderId { get; set; }
    public Guid OrderManagerId { get; set; }
}
