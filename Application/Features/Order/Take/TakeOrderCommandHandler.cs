using Application.Abstractions.Repositories;
using Application.DTOs.Order;
using Common.Enums;
using MediatR;

namespace Application.Features.Order.Take;

public class TakeOrderCommandHandler : IRequestHandler<TakeOrderCommand, OrderResponse>
{
    private readonly IOrdersRepository _repo;

    public TakeOrderCommandHandler(IOrdersRepository repo)
    {
        _repo = repo;
    }

    public async Task<OrderResponse> Handle(TakeOrderCommand request, CancellationToken cancellationToken)
    {
        var active = await _repo.GetActiveAssignedToOrderManagerAsync(request.OrderManagerId, cancellationToken);
        if (active != null) throw new InvalidOperationException("You already have an active delivery.");

        var order = await _repo.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null) throw new KeyNotFoundException("Order not found");

        if (order.AssignedOrderManagerId != null) throw new InvalidOperationException("Order is already assigned.");
        if (order.Status == OrderStatus.Delivered) throw new InvalidOperationException("Delivered orders cannot be taken.");
        if (order.Status == OrderStatus.Cancelled) throw new InvalidOperationException("Cancelled orders cannot be taken.");

        order.AssignedOrderManagerId = request.OrderManagerId;
        order.AssignedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(order, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return OrderResponseFactory.Create(order);
    }
}
