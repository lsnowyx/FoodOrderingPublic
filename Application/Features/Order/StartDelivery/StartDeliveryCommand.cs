using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.StartDelivery;

public sealed class StartDeliveryCommand : IRequest<OrderResponse>
{
    public Guid Id { get; set; }
    public Guid OrderManagerId { get; set; }
}
