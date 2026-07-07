namespace Application.DTOs.DeliveryTracking;

public sealed record DeliveryTrackingLocationUpdate(
    Guid OrderId,
    Guid TrackingSessionId,
    decimal Latitude,
    decimal Longitude,
    decimal Progress,
    string TrackingStatus,
    int EstimatedSecondsRemaining,
    DateTime UpdatedAt);
