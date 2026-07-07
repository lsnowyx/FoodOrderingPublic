using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.DeliveryTracking;
using Common.Enums;
using Domain.ConfigModels;
using Domain.Entities;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class DeliveryTrackingMapService : IDeliveryTrackingMapService
{
    private const string DeliveryNotStartedMessage = "Delivery has not started yet.";
    private const string MissingCoordinatesMessage = "Delivery coordinates are missing for this order.";

    private readonly IDeliveryTrackingAccessService _trackingAccessService;
    private readonly IDeliveryTrackingSessionsRepository _trackingSessionsRepository;
    private readonly DeliverySimulationSettings _settings;

    public DeliveryTrackingMapService(
        IDeliveryTrackingAccessService trackingAccessService,
        IDeliveryTrackingSessionsRepository trackingSessionsRepository,
        IOptions<DeliverySimulationSettings> settings)
    {
        _trackingAccessService = trackingAccessService;
        _trackingSessionsRepository = trackingSessionsRepository;
        _settings = settings.Value;
    }

    public async Task<DeliveryTrackingMapResponse> GetGuestMapAsync(
        string trackingToken,
        CancellationToken cancellationToken = default)
    {
        var order = await _trackingAccessService.GetGuestTrackedOrderAsync(
            trackingToken,
            cancellationToken);

        return await CreateMapResponseAsync(order, cancellationToken);
    }

    public async Task<DeliveryTrackingMapResponse?> GetCustomerMapAsync(
        Guid orderId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var order = await _trackingAccessService.GetCustomerTrackedOrderAsync(
            orderId,
            customerId,
            cancellationToken);

        if (order is null)
        {
            return null;
        }

        return await CreateMapResponseAsync(order, cancellationToken);
    }

    private async Task<DeliveryTrackingMapResponse> CreateMapResponseAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        var trackingSession = await _trackingSessionsRepository.GetLatestByOrderIdAsync(
            order.Id,
            cancellationToken);

        if (trackingSession is null)
        {
            var destinationLatitude = ToDecimal(order.DeliveryLatitude);
            var destinationLongitude = ToDecimal(order.DeliveryLongitude);
            var message = destinationLatitude.HasValue && destinationLongitude.HasValue
                ? DeliveryNotStartedMessage
                : MissingCoordinatesMessage;

            return new DeliveryTrackingMapResponse(
                order.Id,
                _settings.RestaurantLatitude,
                _settings.RestaurantLongitude,
                destinationLatitude,
                destinationLongitude,
                order.DeliveryAddress,
                null,
                null,
                0m,
                "NotStarted",
                null,
                order.UpdatedAt,
                message);
        }

        return new DeliveryTrackingMapResponse(
            order.Id,
            _settings.RestaurantLatitude,
            _settings.RestaurantLongitude,
            trackingSession.DestinationLatitude,
            trackingSession.DestinationLongitude,
            order.DeliveryAddress,
            trackingSession.CurrentLatitude,
            trackingSession.CurrentLongitude,
            trackingSession.Progress,
            trackingSession.Status.ToString(),
            GetEstimatedSecondsRemaining(trackingSession),
            trackingSession.UpdatedAt,
            null);
    }

    private static decimal? ToDecimal(double? value)
    {
        if (!value.HasValue || !double.IsFinite(value.Value))
        {
            return null;
        }

        return (decimal)value.Value;
    }

    private static int? GetEstimatedSecondsRemaining(DeliveryTrackingSession session)
    {
        if (session.Status == DeliveryTrackingStatus.Arrived)
        {
            return 0;
        }

        if (session.DurationSeconds <= 0)
        {
            return 0;
        }

        var remaining = (1m - Math.Clamp(session.Progress, 0m, 1m))
            * session.DurationSeconds;

        return (int)Math.Ceiling((double)Math.Max(remaining, 0m));
    }
}
