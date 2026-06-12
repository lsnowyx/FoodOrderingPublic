using Application.Abstractions.Repositories;
using Application.DTOs.Order;
using Common.Enums;
using MediatR;

namespace Application.Features.Order.MarkDelivered;

public class MarkOrderDeliveredCommandHandler : IRequestHandler<MarkOrderDeliveredCommand, OrderResponse>
{
    private readonly IOrdersRepository _repo;

    public MarkOrderDeliveredCommandHandler(IOrdersRepository repo)
    {
        _repo = repo;
    }

    public async Task<OrderResponse> Handle(MarkOrderDeliveredCommand request, CancellationToken cancellationToken)
    {
        var order = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (order == null) throw new KeyNotFoundException("Order not found");
        OrderAssignmentGuard.EnsureAssignedToOrderManager(order, request.OrderManagerId);

        if (order.Status == OrderStatus.Cancelled) throw new InvalidOperationException("Cannot mark a cancelled order as delivered");
        if (order.Status == OrderStatus.Delivered) return OrderResponseFactory.Create(order); // idempotent
        if (order.Status != OrderStatus.OutForDelivery) throw new InvalidOperationException("Only orders out for delivery can be marked as delivered.");

        order.Status = OrderStatus.Delivered;
        order.DeliveredAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(order, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return OrderResponseFactory.Create(order);
    }
}
