using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Order;
using Common.Enums;
using MediatR;

namespace Application.Features.Order.UpdateStatus;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OrderResponse>
{
    private readonly IOrdersRepository _repo;
    private readonly IOrderCompletionService _orderCompletionService;

    public UpdateOrderStatusCommandHandler(
        IOrdersRepository repo,
        IOrderCompletionService orderCompletionService)
    {
        _repo = repo;
        _orderCompletionService = orderCompletionService;
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

        if (!order.IsPaid && newStatus == OrderStatus.Delivered)
        {
            throw new InvalidOperationException(
                $"Unpaid orders cannot move to {newStatus}.");
        }

        if (newStatus == OrderStatus.Paid
            && order.PaymentMethod == PaymentMethod.OnlineCard
            && (!order.IsPaid || order.PaymentStatus != PaymentStatus.Paid))
        {
            throw new InvalidOperationException("Online card payments must be confirmed by Stripe.");
        }

        if (newStatus == OrderStatus.Paid
            && order.PaymentMethod != PaymentMethod.CashOnDelivery
            && order.PaymentMethod != PaymentMethod.OnlineCard)
        {
            throw new InvalidOperationException("Only orders with a known payment method can be marked as paid.");
        }

        var now = DateTime.UtcNow;

        if (newStatus == OrderStatus.Delivered)
        {
            await _orderCompletionService.MarkDeliveredAsync(order, now, cancellationToken);
        }
        else
        {
            order.Status = newStatus;
            if (newStatus == OrderStatus.Paid)
            {
                order.IsPaid = true;
                order.PaymentStatus = PaymentStatus.Paid;
                order.PaidAt ??= now;
            }

            order.UpdatedAt = now;
        }

        await _repo.UpdateAsync(order, cancellationToken);
        _repo.RecalculateTotalsAsync(order);
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
