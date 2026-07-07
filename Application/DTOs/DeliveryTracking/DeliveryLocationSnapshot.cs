namespace Application.DTOs.DeliveryTracking;

public sealed record DeliveryLocationSnapshot(
    decimal Latitude,
    decimal Longitude,
    decimal Progress,
    int EstimatedSecondsRemaining,
    bool HasArrived);
