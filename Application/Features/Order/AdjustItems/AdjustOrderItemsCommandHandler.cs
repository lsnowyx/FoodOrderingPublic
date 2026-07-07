using Application.Abstractions.Repositories;
using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.AdjustItems;

public class AdjustOrderItemsCommandHandler : IRequestHandler<AdjustOrderItemsCommand, OrderResponse>
{
    private readonly IOrdersRepository _repo;

    public AdjustOrderItemsCommandHandler(IOrdersRepository repo)
    {
        _repo = repo;
    }

    public async Task<OrderResponse> Handle(AdjustOrderItemsCommand request, CancellationToken cancellationToken)
    {
        var order = await _repo.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null) throw new KeyNotFoundException("Order not found");
        OrderAssignmentGuard.EnsureAssignedToOrderManager(order, request.OrderManagerId);
        OrderAssignmentGuard.EnsureEditable(order);
        if (request.OrderManagerId != null) throw new UnauthorizedAccessException("Order managers cannot change order item quantities.");

        OrderItemQuantityGuard.EnsureValidOrderShape(
            request.Items,
            item => item.Quantity);

        foreach (var item in request.Items)
        {
            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == item.ItemId);
            if (orderItem == null) throw new KeyNotFoundException($"Order item {item.ItemId} not found");
            orderItem.Quantity = item.Quantity;
        }

        _repo.RecalculateTotalsAsync(order);
        await _repo.UpdateAsync(order, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return OrderResponseFactory.Create(order);
    }
}
