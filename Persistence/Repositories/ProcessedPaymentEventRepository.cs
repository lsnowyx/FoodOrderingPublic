using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public sealed class ProcessedPaymentEventRepository : IProcessedPaymentEventRepository
{
    private readonly AppDbContext context;

    public ProcessedPaymentEventRepository(AppDbContext context)
    {
        this.context = context;
    }

    public Task<bool> ExistsAsync(
        string provider,
        string providerEventId,
        CancellationToken cancellationToken = default)
    {
        return context.ProcessedPaymentEvents
            .AsNoTracking()
            .AnyAsync(
                processedPaymentEvent =>
                    processedPaymentEvent.Provider == provider
                    && processedPaymentEvent.ProviderEventId == providerEventId,
                cancellationToken);
    }

    public async Task AddAsync(
        ProcessedPaymentEvent processedPaymentEvent,
        CancellationToken cancellationToken = default)
    {
        await context.ProcessedPaymentEvents.AddAsync(processedPaymentEvent, cancellationToken);
    }
}
