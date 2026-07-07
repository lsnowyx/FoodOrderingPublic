namespace Application.DTOs.DeliveryTracking;

public sealed record DeliveryTrackingMapResponse(
    Guid OrderId,
    decimal RestaurantLatitude,
    decimal RestaurantLongitude,
    decimal? DestinationLatitude,
    decimal? DestinationLongitude,
    string? DestinationAddress,
    decimal? CourierLatitude,
    decimal? CourierLongitude,
    decimal Progress,
    string TrackingStatus,
    int? EstimatedSecondsRemaining,
    DateTime? UpdatedAt,
    string? Message);
