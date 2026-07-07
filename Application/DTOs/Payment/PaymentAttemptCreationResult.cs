namespace Application.DTOs.Payment;

public sealed record PaymentAttemptCreationResult(
    string PaymentUrl,
    string CheckoutSessionId);
