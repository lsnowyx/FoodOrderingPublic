using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.DeliveryTracking;
using Common.Enums;
using Domain.ConfigModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class DeliverySimulationJob : IDeliverySimulationJob
{
    private const int SafeDefaultUpdateIntervalSeconds = 5;

    private readonly IDeliveryTrackingSessionsRepository _trackingSessionsRepository;
    private readonly IDeliveryLocationCalculator _deliveryLocationCalculator;
    private readonly IDeliveryTrackingBroadcaster _deliveryTrackingBroadcaster;
    private readonly ILogger<DeliverySimulationJob> _logger;
    private readonly DeliverySimulationSettings _settings;

    public DeliverySimulationJob(
        IDeliveryTrackingSessionsRepository trackingSessionsRepository,
        IDeliveryLocationCalculator deliveryLocationCalculator,
        IDeliveryTrackingBroadcaster deliveryTrackingBroadcaster,
        ILogger<DeliverySimulationJob> logger,
        IOptions<DeliverySimulationSettings> settings)
    {
        _trackingSessionsRepository = trackingSessionsRepository;
        _deliveryLocationCalculator = deliveryLocationCalculator;
        _deliveryTrackingBroadcaster = deliveryTrackingBroadcaster;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task RunAsync(Guid trackingSessionId, CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var session = await _trackingSessionsRepository.GetByIdAsync(
                trackingSessionId,
                cancellationToken);

            if (session is null || session.Status != DeliveryTrackingStatus.InProgress)
            {
                return;
            }

            var now = DateTime.UtcNow;
            var snapshot = _deliveryLocationCalculator.Calculate(
                new DeliveryLocationCalculationRequest(
                    session.StartLatitude,
                    session.StartLongitude,
                    session.DestinationLatitude,
                    session.DestinationLongitude,
                    session.StartedAt,
                    session.DurationSeconds,
                    now));

            ApplySnapshot(session, snapshot, now);

            await _trackingSessionsRepository.SaveChangesAsync(cancellationToken);
            await BroadcastLocationUpdatedAsync(session, snapshot, cancellationToken);

            if (snapshot.HasArrived)
            {
                return;
            }

            var updateIntervalSeconds = GetUpdateIntervalSeconds(session.UpdateIntervalSeconds);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(updateIntervalSeconds), cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private static void ApplySnapshot(
        Domain.Entities.DeliveryTrackingSession session,
        DeliveryLocationSnapshot snapshot,
        DateTime now)
    {
        session.CurrentLatitude = snapshot.HasArrived
            ? session.DestinationLatitude
            : snapshot.Latitude;
        session.CurrentLongitude = snapshot.HasArrived
            ? session.DestinationLongitude
            : snapshot.Longitude;
        session.Progress = snapshot.HasArrived ? 1m : snapshot.Progress;
        session.UpdatedAt = now;

        if (!snapshot.HasArrived)
        {
            return;
        }

        session.Status = DeliveryTrackingStatus.Arrived;
        session.ArrivedAt ??= now;
    }

    private async Task BroadcastLocationUpdatedAsync(
        Domain.Entities.DeliveryTrackingSession session,
        DeliveryLocationSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        try
        {
            await _deliveryTrackingBroadcaster.BroadcastLocationUpdatedAsync(
                new DeliveryTrackingLocationUpdate(
                    session.OrderId,
                    session.Id,
                    session.CurrentLatitude,
                    session.CurrentLongitude,
                    session.Progress,
                    session.Status.ToString(),
                    snapshot.HasArrived ? 0 : snapshot.EstimatedSecondsRemaining,
                    session.UpdatedAt ?? DateTime.UtcNow),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Could not broadcast delivery tracking update for order {OrderId}.",
                session.OrderId);
        }
    }

    private int GetUpdateIntervalSeconds(int sessionUpdateIntervalSeconds)
    {
        if (sessionUpdateIntervalSeconds > 0)
        {
            return sessionUpdateIntervalSeconds;
        }

        if (_settings.UpdateIntervalSeconds > 0)
        {
            return _settings.UpdateIntervalSeconds;
        }

        return SafeDefaultUpdateIntervalSeconds;
    }
}
