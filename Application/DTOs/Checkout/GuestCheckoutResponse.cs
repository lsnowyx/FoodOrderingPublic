namespace Application.DTOs.Checkout;

public class GuestCheckoutResponse
{
    public Guid OrderId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public decimal Total { get; set; }

    public string TrackingToken { get; set; } = null!;

    public string? PaymentUrl { get; set; }
}
