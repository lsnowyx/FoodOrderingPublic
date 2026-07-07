namespace Application.DTOs.Order;

public sealed record CreateOrderResponse(
    Guid OrderId,
    string Status,
    bool IsPaid,
    string PaymentMethod,
    string PaymentStatus,
    decimal TotalAmount,
    string? PaymentUrl);
