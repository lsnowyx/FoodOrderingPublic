using Application.Abstractions.Services;
using Application.DTOs.DeliveryTracking;

namespace Infrastructure.Services;

public sealed class DeliveryLocationCalculator : IDeliveryLocationCalculator
{
    public DeliveryLocationSnapshot Calculate(DeliveryLocationCalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.DurationSeconds <= 0)
        {
            return new DeliveryLocationSnapshot(
                request.DestinationLatitude,
                request.DestinationLongitude,
                1m,
                0,
                true);
        }

        var elapsedSeconds = (decimal)Math.Max((request.Now - request.StartedAt).TotalSeconds, 0d);
        var progress = Math.Clamp(elapsedSeconds / request.DurationSeconds, 0m, 1m);
        var currentLatitude = request.StartLatitude
            + (request.DestinationLatitude - request.StartLatitude) * progress;
        var currentLongitude = request.StartLongitude
            + (request.DestinationLongitude - request.StartLongitude) * progress;
        var estimatedSecondsRemaining = progress >= 1m
            ? 0
            : (int)Math.Ceiling((double)Math.Max(request.DurationSeconds - elapsedSeconds, 0m));

        return new DeliveryLocationSnapshot(
            currentLatitude,
            currentLongitude,
            progress,
            estimatedSecondsRemaining,
            progress >= 1m);
    }
}
