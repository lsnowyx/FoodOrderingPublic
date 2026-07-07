using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.GetCustomerPaymentLink;

public sealed class GetCustomerPaymentLinkCommand : IRequest<CustomerPaymentUrlStatusResponse>
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
}
