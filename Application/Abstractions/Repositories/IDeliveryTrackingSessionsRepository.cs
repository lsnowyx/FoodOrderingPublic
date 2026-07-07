using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IDeliveryTrackingSessionsRepository
{
    Task<DeliveryTrackingSession?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<DeliveryTrackingSession?> GetActiveByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<DeliveryTrackingSession?> GetLatestByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<DeliveryTrackingSession> AddAsync(
        DeliveryTrackingSession session,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        DeliveryTrackingSession session,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
