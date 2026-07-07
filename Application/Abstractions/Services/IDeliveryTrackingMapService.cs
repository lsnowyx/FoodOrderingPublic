using Application.DTOs.DeliveryTracking;

namespace Application.Abstractions.Services;

public interface IDeliveryTrackingMapService
{
    Task<DeliveryTrackingMapResponse> GetGuestMapAsync(
        string trackingToken,
        CancellationToken cancellationToken = default);

    Task<DeliveryTrackingMapResponse?> GetCustomerMapAsync(
        Guid orderId,
        Guid customerId,
        CancellationToken cancellationToken = default);
}
