using Application.Abstractions.Repositories;
using Application.DTOs.Order;
using Common.Enums;
using MediatR;

namespace Application.Features.Order.UpdatePayment;

public class UpdateOrderPaymentCommandHandler : IRequestHandler<UpdateOrderPaymentCommand, OrderResponse>
{
    private readonly IOrdersRepository _repo;

    public UpdateOrderPaymentCommandHandler(IOrdersRepository repo)
    {
        _repo = repo;
    }

    public async Task<OrderResponse> Handle(UpdateOrderPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (order == null) throw new KeyNotFoundException("Order not found");
        OrderAssignmentGuard.EnsureAssignedToOrderManager(order, request.OrderManagerId);
        OrderAssignmentGuard.EnsureEditable(order);

        if (!request.IsPaid
            && (order.IsPaid
                || order.Status == OrderStatus.Paid
                || order.Status == OrderStatus.Preparing
                || order.Status == OrderStatus.OutForDelivery))
            throw new InvalidOperationException("Paid orders cannot be marked as unpaid.");

        if (request.IsPaid && order.PaymentMethod == PaymentMethod.OnlineCard)
        {
            throw new InvalidOperationException("Online card payments must be confirmed by Stripe.");
        }

        if (request.IsPaid && order.PaymentMethod != PaymentMethod.CashOnDelivery)
        {
            throw new InvalidOperationException("Only cash on delivery orders can be manually marked as paid.");
        }

        var now = DateTime.UtcNow;

        if (request.IsPaid)
        {
            order.IsPaid = true;
            order.PaymentStatus = PaymentStatus.Paid;
            order.PaidAt ??= now;

            if (order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Paid;
            }
        }
        else
        {
            order.IsPaid = false;

            if (order.PaymentMethod == PaymentMethod.CashOnDelivery)
            {
                order.PaymentStatus = PaymentStatus.Unpaid;
                order.PaidAt = null;
            }
        }

        order.UpdatedAt = now;

        await _repo.UpdateAsync(order, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return OrderResponseFactory.Create(order);
    }
}
