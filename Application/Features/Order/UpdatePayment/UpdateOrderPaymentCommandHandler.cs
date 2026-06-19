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

        order.IsPaid = request.IsPaid;
        if (request.IsPaid && order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.Paid;
        }

        order.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(order, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return OrderResponseFactory.Create(order);
    }
}
