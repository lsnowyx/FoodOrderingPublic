using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IPaymentAttemptRepository
{
    Task AddPaymentAttemptAsync(PaymentAttempt paymentAttempt, CancellationToken cancellationToken);
    Task<Order?> GetOrderForPaymentRetryUpdateAsync(Guid orderId, CancellationToken cancellationToken);
    Task<PaymentAttempt?> GetPaymentAttemptWithOrderAsync(Guid paymentAttemptId, CancellationToken cancellationToken);
    Task<PaymentAttempt?> GetPaymentAttemptByStripeCheckoutSessionIdWithOrderAsync(
        string stripeCheckoutSessionId,
        CancellationToken cancellationToken);
    Task<PaymentAttempt?> GetPaymentAttemptByStripePaymentIntentIdWithOrderAsync(
        string stripePaymentIntentId,
        CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
