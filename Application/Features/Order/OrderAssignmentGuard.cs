using Common.Enums;

namespace Application.Features.Order;

internal static class OrderAssignmentGuard
{
    public static void EnsureAssignedToOrderManager(Domain.Entities.Order order, Guid? orderManagerId)
    {
        if (orderManagerId == null) return;

        if (order.AssignedOrderManagerId != orderManagerId)
            throw new UnauthorizedAccessException("Order is not assigned to the current order manager.");
    }

    public static void EnsureEditable(Domain.Entities.Order order)
    {
        if (order.Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Delivered orders cannot be edited.");

        if (order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Cancelled orders cannot be edited.");
    }
}
