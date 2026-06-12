using Application.Abstractions.Repositories;
using Application.DTOs.Order;
using Common.Enums;
using MediatR;

namespace Application.Features.Order.StartDelivery;

public class StartDeliveryCommandHandler : IRequestHandler<StartDeliveryCommand, OrderResponse>
{
    private readonly IOrdersRepository _repo;

    public StartDeliveryCommandHandler(IOrdersRepository repo)
    {
        _repo = repo;
    }

    public async Task<OrderResponse> Handle(StartDeliveryCommand request, CancellationToken cancellationToken)
    {
        var order = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (order == null) throw new KeyNotFoundException("Order not found");

        OrderAssignmentGuard.EnsureAssignedToOrderManager(order, request.OrderManagerId);
        OrderAssignmentGuard.EnsureEditable(order);

        order.Status = OrderStatus.OutForDelivery;
        order.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(order, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return OrderResponseFactory.Create(order);
    }
}
