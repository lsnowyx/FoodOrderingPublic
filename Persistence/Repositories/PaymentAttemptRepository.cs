using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories
{
    public class PaymentAttemptRepository : IPaymentAttemptRepository
    {
        private readonly AppDbContext _context;

        public PaymentAttemptRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddPaymentAttemptAsync(PaymentAttempt paymentAttempt, CancellationToken cancellationToken)
        {
            await _context.PaymentAttempts.AddAsync(paymentAttempt, cancellationToken);
        }

        public async Task<Order?> GetOrderForPaymentRetryUpdateAsync(Guid orderId, CancellationToken cancellationToken)
        {
            DetachTrackedPaymentRetryEntities(orderId);

            var order = await _context.Orders
                .FromSqlInterpolated($"SELECT * FROM [Orders] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {orderId}")
                .FirstOrDefaultAsync(cancellationToken);

            if (order is null)
            {
                return null;
            }

            await _context.Entry(order)
                .Collection(retryOrder => retryOrder.PaymentAttempts)
                .LoadAsync(cancellationToken);

            await _context.Entry(order)
                .Reference(retryOrder => retryOrder.User)
                .LoadAsync(cancellationToken);

            await _context.Entry(order)
                .Reference(retryOrder => retryOrder.GuestCustomer)
                .LoadAsync(cancellationToken);

            return order;
        }

        public Task<PaymentAttempt?> GetPaymentAttemptWithOrderAsync(Guid paymentAttemptId, CancellationToken cancellationToken)
        {
            return GetPaymentAttemptsWithOrder()
                .FirstOrDefaultAsync(
                    paymentAttempt => paymentAttempt.Id == paymentAttemptId,
                    cancellationToken);
        }

        public Task<PaymentAttempt?> GetPaymentAttemptByStripeCheckoutSessionIdWithOrderAsync(
            string stripeCheckoutSessionId,
            CancellationToken cancellationToken)
        {
            return GetPaymentAttemptsWithOrder()
                .FirstOrDefaultAsync(
                    paymentAttempt => paymentAttempt.StripeCheckoutSessionId == stripeCheckoutSessionId,
                    cancellationToken);
        }

        public Task<PaymentAttempt?> GetPaymentAttemptByStripePaymentIntentIdWithOrderAsync(
            string stripePaymentIntentId,
            CancellationToken cancellationToken)
        {
            return GetPaymentAttemptsWithOrder()
                .FirstOrDefaultAsync(
                    paymentAttempt => paymentAttempt.StripePaymentIntentId == stripePaymentIntentId,
                    cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        private IQueryable<PaymentAttempt> GetPaymentAttemptsWithOrder()
        {
            return _context.PaymentAttempts
                .Include(paymentAttempt => paymentAttempt.Order)
                    .ThenInclude(order => order.PaymentAttempts);
        }

        private void DetachTrackedPaymentRetryEntities(Guid orderId)
        {
            foreach (var paymentAttemptEntry in _context.ChangeTracker
                         .Entries<PaymentAttempt>()
                         .Where(entry => entry.Entity.OrderId == orderId)
                         .ToList())
            {
                paymentAttemptEntry.State = EntityState.Detached;
            }

            var orderEntry = _context.ChangeTracker
                .Entries<Order>()
                .FirstOrDefault(entry => entry.Entity.Id == orderId);

            if (orderEntry is not null)
            {
                orderEntry.State = EntityState.Detached;
            }
        }
    }
}
