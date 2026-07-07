namespace Application.DTOs.Order;

public sealed record CustomerOrderDetailsResponse(
    Guid OrderId,
    DateTime CreatedAt,
    string OrderStatus,
    string PaymentMethod,
    string PaymentStatus,
    bool IsPaid,
    decimal TotalAmount,
    DateTime? PaidAt,
    DateTime? DeliveredAt,
    DateTime? UpdatedAt,
    string? DeliveryAddress,
    double? DeliveryLatitude,
    double? DeliveryLongitude,
    bool CanPayOnlineAgain,
    string PaymentMessage,
    IReadOnlyList<CustomerOrderItemResponse> Items);
