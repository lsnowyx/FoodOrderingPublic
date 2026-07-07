namespace Application.DTOs.Order;

public sealed record CustomerOrderSummaryResponse(
    Guid OrderId,
    DateTime CreatedAt,
    string OrderStatus,
    string PaymentMethod,
    string PaymentStatus,
    bool IsPaid,
    decimal TotalAmount,
    int ItemsCount,
    bool CanPayOnlineAgain,
    string PaymentMessage,
    DateTime? DeliveredAt);
