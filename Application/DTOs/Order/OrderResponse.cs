namespace Application.DTOs.Order;

public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    Guid? AssignedOrderManagerId,
    DateTime? AssignedAt,
    DateTime OrderDate,
    string Status,
    bool IsPaid,
    string? DeliveryAddress,
    DateTime? DeliveredAt,
    DateTime? UpdatedAt,
    IEnumerable<OrderItemDto> Items);
