namespace Application.DTOs.Order;

public sealed record CustomerPaymentUrlStatusResponse(
    bool IsPaid,
    bool CanPayOnlineAgain,
    string? PaymentUrl,
    string Message,
    string PaymentStatus);
