using Application.Abstractions.Repositories;
using Application.DTOs.Order;
using Common.Enums;
using MediatR;

namespace Application.Features.Order.UpdateStatus;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OrderResponse>
{
    private readonly IOrdersRepository _repo;

    public UpdateOrderStatusCommandHandler(IOrdersRepository repo)
    {
        _repo = repo;
    }

    public async Task<OrderResponse> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (order == null) throw new KeyNotFoundException("Order not found");
        OrderAssignmentGuard.EnsureAssignedToOrderManager(order, request.OrderManagerId);
        OrderAssignmentGuard.EnsureEditable(order);

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus)) throw new ArgumentException("Invalid status");

        var current = order.Status;

        if (!IsValidStatusTransition(current, newStatus))
            throw new ArgumentException($"Invalid status transition from {current} to {newStatus}");

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;
        if (newStatus == OrderStatus.Delivered)
        {
            order.DeliveredAt = DateTime.UtcNow;
        }

        await _repo.UpdateAsync(order, cancellationToken);
        await _repo.RecalculateTotalsAsync(order, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return OrderResponseFactory.Create(order);
    }

    private static bool IsValidStatusTransition(OrderStatus current, OrderStatus next)
    {
        if (current == next) return true;

        return current switch
        {
            OrderStatus.Pending => next == OrderStatus.Paid || next == OrderStatus.Cancelled,
            OrderStatus.Paid => next == OrderStatus.Preparing || next == OrderStatus.Cancelled,
            OrderStatus.Preparing => next == OrderStatus.OutForDelivery || next == OrderStatus.Cancelled,
            OrderStatus.OutForDelivery => next == OrderStatus.Delivered || next == OrderStatus.Cancelled,
            OrderStatus.Delivered => false,
            OrderStatus.Cancelled => false,
            _ => false
        };
    }
}
