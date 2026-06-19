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
        var assigned = await _repo.TryAssignAsync(
            request.OrderId,
            request.OrderManagerId,
            DateTime.UtcNow,
            cancellationToken);

        if (!assigned)
        {
            var active = await _repo.GetActiveAssignedToOrderManagerAsync(
                request.OrderManagerId,
                cancellationToken);
            if (active is not null)
            {
                throw new InvalidOperationException("You already have an active delivery.");
            }

            var unavailableOrder = await _repo.GetByIdAsync(request.OrderId, cancellationToken);
            if (unavailableOrder is null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            if (unavailableOrder.AssignedOrderManagerId is not null)
            {
                throw new InvalidOperationException("Order is already assigned.");
            }

            if (unavailableOrder.Status == OrderStatus.Delivered)
            {
                throw new InvalidOperationException("Delivered orders cannot be taken.");
            }

            if (unavailableOrder.Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Cancelled orders cannot be taken.");
            }

            throw new InvalidOperationException("Order could not be assigned. Please try again.");
        }

        var order = await _repo.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException("Order not found after assignment.");

        return OrderResponseFactory.Create(order);
    }
}
