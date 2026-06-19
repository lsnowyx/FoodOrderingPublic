using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.GetAssignedById;

public sealed class GetAssignedOrderByIdQuery : IRequest<OrderResponse?>
{
    public Guid Id { get; set; }
    public Guid OrderManagerId { get; set; }
}
