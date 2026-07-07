using Application.DTOs.DeliveryTracking;

namespace Application.Abstractions.Services;

public interface IDeliveryTrackingBroadcaster
{
    Task BroadcastLocationUpdatedAsync(
        DeliveryTrackingLocationUpdate update,
        CancellationToken cancellationToken = default);
}
