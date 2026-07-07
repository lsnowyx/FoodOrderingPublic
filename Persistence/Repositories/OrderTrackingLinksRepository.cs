using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class OrderTrackingLinksRepository : IOrderTrackingLinksRepository
{
    private readonly AppDbContext _context;

    public OrderTrackingLinksRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderTrackingLink?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _context.OrderTrackingLinks
            .Include(x => x.Order).ThenInclude(o => o.GuestCustomer)
            .Include(x => x.Order).ThenInclude(o => o.PaymentAttempts)
            .Include(x => x.Order).ThenInclude(o => o.OrderItems)
                .ThenInclude(orderItem => orderItem.MenuItem)
            .FirstOrDefaultAsync(trackingLink => trackingLink.TokenHash == tokenHash, cancellationToken);
    }

    public async Task RevokeActiveByOrderIdAsync(Guid orderId, DateTime revokedAt, CancellationToken cancellationToken = default)
    {
        var trackingLinks = await _context.OrderTrackingLinks
            .Where(trackingLink => trackingLink.OrderId == orderId && trackingLink.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var trackingLink in trackingLinks)
        {
            trackingLink.RevokedAt = revokedAt;
        }
    }

    public Task UpdateAsync(OrderTrackingLink trackingLink, CancellationToken cancellationToken = default)
    {
        _context.Entry(trackingLink)
            .Property(link => link.LastUsedAt)
            .IsModified = true;

        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
