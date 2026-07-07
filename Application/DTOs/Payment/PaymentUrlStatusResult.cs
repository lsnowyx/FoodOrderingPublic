namespace Application.DTOs.Payment;

public sealed class PaymentUrlStatusResult
{
    public bool IsPaid { get; set; }

    public string? PaymentUrl { get; set; }

    public string Message { get; set; } = null!;
}
