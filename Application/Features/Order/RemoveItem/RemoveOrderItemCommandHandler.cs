using Application.Abstractions.Repositories;
using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.RemoveItem;

public class RemoveOrderItemCommandHandler : IRequestHandler<RemoveOrderItemCommand, OrderResponse>
{
    private readonly IOrdersRepository _repo;

    public RemoveOrderItemCommandHandler(IOrdersRepository repo)
    {
        _repo = repo;
    }

    public async Task<OrderResponse> Handle(RemoveOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await _repo.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null) throw new KeyNotFoundException("Order not found");
        OrderAssignmentGuard.EnsureAssignedToOrderManager(order, request.OrderManagerId);
        OrderAssignmentGuard.EnsureEditable(order);
        if (request.OrderManagerId != null) throw new UnauthorizedAccessException("Order managers cannot remove order items.");

        var item = order.OrderItems.FirstOrDefault(oi => oi.Id == request.ItemId);
        if (item == null) throw new KeyNotFoundException("Order item not found");

        order.OrderItems.Remove(item);
        _repo.RecalculateTotalsAsync(order);
        await _repo.UpdateAsync(order, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return OrderResponseFactory.Create(order);
    }
}
