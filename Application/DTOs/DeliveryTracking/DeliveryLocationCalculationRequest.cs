namespace Application.DTOs.DeliveryTracking;

public sealed record DeliveryLocationCalculationRequest(
    decimal StartLatitude,
    decimal StartLongitude,
    decimal DestinationLatitude,
    decimal DestinationLongitude,
    DateTime StartedAt,
    int DurationSeconds,
    DateTime Now);
