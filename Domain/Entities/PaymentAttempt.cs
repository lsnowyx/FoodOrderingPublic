using Common.Enums;

namespace Domain.Entities;

public class PaymentAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;

    public string? StripeCheckoutSessionId { get; set; }
    public string? StripeCheckoutUrl { get; set; }
    public string? StripePaymentIntentId { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Unpaid;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
