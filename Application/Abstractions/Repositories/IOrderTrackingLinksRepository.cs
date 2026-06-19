using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IOrderTrackingLinksRepository
{
    Task<OrderTrackingLink?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task RevokeActiveByOrderIdAsync(Guid orderId, DateTime revokedAt, CancellationToken cancellationToken = default);

    Task UpdateAsync(OrderTrackingLink trackingLink, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
