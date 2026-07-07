using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.GetCustomerOrderDetails;

public sealed class GetCustomerOrderDetailsQuery : IRequest<CustomerOrderDetailsResponse?>
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
}
