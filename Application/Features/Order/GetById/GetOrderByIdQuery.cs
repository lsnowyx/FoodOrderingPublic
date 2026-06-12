using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.GetById;

public sealed class GetOrderByIdQuery : IRequest<OrderResponse?>
{
    public Guid Id { get; set; }
}
