using Application.Abstractions.Repositories;
using Common.Enums;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class DeliveryTrackingSessionsRepository : IDeliveryTrackingSessionsRepository
{
    private readonly AppDbContext _context;

    public DeliveryTrackingSessionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<DeliveryTrackingSession?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.DeliveryTrackingSessions
            .FirstOrDefaultAsync(session => session.Id == id, cancellationToken);
    }

    public Task<DeliveryTrackingSession?> GetActiveByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return _context.DeliveryTrackingSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                session => session.OrderId == orderId
                    && session.Status == DeliveryTrackingStatus.InProgress,
                cancellationToken);
    }

    public Task<DeliveryTrackingSession?> GetLatestByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return _context.DeliveryTrackingSessions
            .AsNoTracking()
            .Where(session => session.OrderId == orderId)
            .OrderByDescending(session => session.CreatedAt)
            .ThenByDescending(session => session.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DeliveryTrackingSession> AddAsync(
        DeliveryTrackingSession session,
        CancellationToken cancellationToken = default)
    {
        await _context.DeliveryTrackingSessions.AddAsync(session, cancellationToken);
        return session;
    }

    public Task UpdateAsync(
        DeliveryTrackingSession session,
        CancellationToken cancellationToken = default)
    {
        _context.DeliveryTrackingSessions.Update(session);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
