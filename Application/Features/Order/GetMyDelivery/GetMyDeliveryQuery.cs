using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.GetMyDelivery;

public sealed class GetMyDeliveryQuery : IRequest<OrderResponse?>
{
    public Guid OrderManagerId { get; set; }
}
