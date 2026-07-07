using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Order;
using Common.Enums;
using MediatR;

namespace Application.Features.Order.MarkDelivered;

public class MarkOrderDeliveredCommandHandler : IRequestHandler<MarkOrderDeliveredCommand, OrderResponse>
{
    private readonly IOrdersRepository _repo;
    private readonly IOrderCompletionService _orderCompletionService;

    public MarkOrderDeliveredCommandHandler(
        IOrdersRepository repo,
        IOrderCompletionService orderCompletionService)
    {
        _repo = repo;
        _orderCompletionService = orderCompletionService;
    }

    public async Task<OrderResponse> Handle(MarkOrderDeliveredCommand request, CancellationToken cancellationToken)
    {
        var order = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (order == null) throw new KeyNotFoundException("Order not found");
        OrderAssignmentGuard.EnsureAssignedToOrderManager(order, request.OrderManagerId);

        if (order.Status != OrderStatus.OutForDelivery)
            throw new InvalidOperationException("Only orders out for delivery can be marked as delivered.");

        if (!order.IsPaid)
            throw new InvalidOperationException("Unpaid orders cannot be marked as delivered.");

        await _orderCompletionService.MarkDeliveredAsync(order, DateTime.UtcNow, cancellationToken);

        await _repo.UpdateAsync(order, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return OrderResponseFactory.Create(order);
    }
}
