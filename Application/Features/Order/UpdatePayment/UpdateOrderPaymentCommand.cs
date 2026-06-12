using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.UpdatePayment;

public sealed class UpdateOrderPaymentCommand : IRequest<OrderResponse>
{
    public Guid Id { get; set; }
    public Guid? OrderManagerId { get; set; }
    public bool IsPaid { get; set; }
}
