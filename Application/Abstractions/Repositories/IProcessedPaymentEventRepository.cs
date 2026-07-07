using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IProcessedPaymentEventRepository
{
    Task<bool> ExistsAsync(
        string provider,
        string providerEventId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ProcessedPaymentEvent processedPaymentEvent,
        CancellationToken cancellationToken = default);
}
