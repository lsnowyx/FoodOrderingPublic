namespace Application.DTOs.Payment;

public class CreateStripeCheckoutSessionRequest
{
    public Guid OrderId { get; set; }

    public Guid PaymentAttemptId { get; set; }

    public string CustomerEmail { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Description { get; set; } = "Restaurant order";

    public string SuccessUrl { get; set; } = null!;

    public string CancelUrl { get; set; } = null!;
}
