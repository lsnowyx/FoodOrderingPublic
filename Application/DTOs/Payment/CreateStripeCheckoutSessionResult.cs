namespace Application.DTOs.Payment;

public sealed class CreateStripeCheckoutSessionResult
{
    public string SessionId { get; set; } = null!;
    public string PaymentUrl { get; set; } = null!;
    public DateTime? ExpiresAt { get; set; }
}
