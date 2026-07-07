namespace Domain.Entities;

public class ProcessedPaymentEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Provider { get; set; } = string.Empty;
    public string ProviderEventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;

    public Guid? PaymentAttemptId { get; set; }
    public PaymentAttempt? PaymentAttempt { get; set; }

    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }

    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string? RawStatus { get; set; }
}
