using Application.DTOs.Payment;
using Domain.Entities;

namespace Application.Abstractions.Services;

public interface IStripePaymentService
{
    Task<PaymentAttemptCreationResult> CreateNewPaymentAttemptForOrderAsync(
        Order order,
        CancellationToken cancellationToken);

    Task ExpireCheckoutSessionAsync(
        string checkoutSessionId,
        CancellationToken cancellationToken);

    Task HandleWebhookAsync(string json, string stripeSignature, CancellationToken cancellationToken);
    Task<PaymentUrlStatusResult> PaymentUrlStatusAsync(Order order, CancellationToken cancellationToken);
}
