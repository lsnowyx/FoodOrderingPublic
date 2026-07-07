using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Common.Enums;
using Domain.ConfigModels;
using Domain.Entities;
using Microsoft.Extensions.Options;
using OrderEntity = Domain.Entities.Order;

namespace Infrastructure.Services;

public sealed class StartDeliveryService : IStartDeliveryService
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IDeliveryTrackingSessionsRepository _trackingSessionsRepository;
    private readonly IDeliverySimulationScheduler _deliverySimulationScheduler;
    private readonly IApplicationTransaction _transaction;
    private readonly DeliverySimulationSettings _settings;

    public StartDeliveryService(
        IOrdersRepository ordersRepository,
        IDeliveryTrackingSessionsRepository trackingSessionsRepository,
        IDeliverySimulationScheduler deliverySimulationScheduler,
        IApplicationTransaction transaction,
        IOptions<DeliverySimulationSettings> settings)
    {
        _ordersRepository = ordersRepository;
        _trackingSessionsRepository = trackingSessionsRepository;
        _deliverySimulationScheduler = deliverySimulationScheduler;
        _transaction = transaction;
        _settings = settings.Value;
    }

    public async Task<OrderEntity> StartAsync(
        Guid orderId,
        Guid orderManagerId,
        CancellationToken cancellationToken = default)
    {
        var result = await _transaction.ExecuteAsync(
            async transactionCancellationToken =>
            {
                var order = await _ordersRepository.GetByIdAsync(
                    orderId,
                    transactionCancellationToken);
                if (order == null) throw new KeyNotFoundException("Order not found");

                EnsureAssignedToOrderManager(order, orderManagerId);
                EnsureEditable(order);

                var activeTrackingSession = await _trackingSessionsRepository.GetActiveByOrderIdAsync(
                    order.Id,
                    transactionCancellationToken);
                if (activeTrackingSession is not null)
                {
                    await EnsureOrderIsOutForDeliveryAsync(
                        order,
                        transactionCancellationToken);

                    return new StartDeliveryTrackingResult(
                        order,
                        activeTrackingSession,
                        false,
                        null);
                }

                EnsureStartableStatus(order);
                EnsureDeliveryCoordinates(order);

                var now = DateTime.UtcNow;
                var previousStatus = order.Status;
                var trackingSession = CreateTrackingSession(order, now);

                order.Status = OrderStatus.OutForDelivery;
                order.UpdatedAt = now;

                await _trackingSessionsRepository.AddAsync(
                    trackingSession,
                    transactionCancellationToken);
                await _ordersRepository.UpdateAsync(order, transactionCancellationToken);
                await _ordersRepository.SaveChangesAsync(transactionCancellationToken);

                return new StartDeliveryTrackingResult(
                    order,
                    trackingSession,
                    true,
                    previousStatus);
            },
            cancellationToken);

        if (result.ShouldEnqueueSimulationJob)
        {
            try
            {
                _deliverySimulationScheduler.Enqueue(result.TrackingSession.Id);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                await CancelStartedDeliveryAsync(result, cancellationToken);
                throw new InvalidOperationException(
                    "Could not start delivery simulation. Please try again.",
                    exception);
            }
        }

        return result.Order;
    }

    private async Task EnsureOrderIsOutForDeliveryAsync(
        OrderEntity order,
        CancellationToken cancellationToken)
    {
        if (order.Status == OrderStatus.OutForDelivery)
        {
            return;
        }

        EnsureStartableStatus(order);

        order.Status = OrderStatus.OutForDelivery;
        order.UpdatedAt = DateTime.UtcNow;

        await _ordersRepository.UpdateAsync(order, cancellationToken);
        await _ordersRepository.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureAssignedToOrderManager(
        OrderEntity order,
        Guid? orderManagerId)
    {
        if (orderManagerId == null)
        {
            return;
        }

        if (order.AssignedOrderManagerId != orderManagerId)
        {
            throw new UnauthorizedAccessException(
                "Order is not assigned to the current order manager.");
        }
    }

    private static void EnsureEditable(OrderEntity order)
    {
        if (order.Status == OrderStatus.Delivered)
        {
            throw new InvalidOperationException("Delivered orders cannot be edited.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled orders cannot be edited.");
        }
    }

    private static void EnsureStartableStatus(OrderEntity order)
    {
        if (order.Status is not OrderStatus.Pending
            and not OrderStatus.Paid
            and not OrderStatus.Preparing)
        {
            throw new InvalidOperationException(
                "Only pending, paid, or preparing orders can start delivery.");
        }
    }

    private static void EnsureDeliveryCoordinates(OrderEntity order)
    {
        if (!order.DeliveryLatitude.HasValue || !order.DeliveryLongitude.HasValue)
        {
            throw new InvalidOperationException(
                "Delivery coordinates are missing for this order.");
        }
    }

    private DeliveryTrackingSession CreateTrackingSession(OrderEntity order, DateTime now)
    {
        var startLatitude = _settings.RestaurantLatitude;
        var startLongitude = _settings.RestaurantLongitude;

        return new DeliveryTrackingSession
        {
            OrderId = order.Id,
            StartLatitude = startLatitude,
            StartLongitude = startLongitude,
            DestinationLatitude = (decimal)order.DeliveryLatitude!.Value,
            DestinationLongitude = (decimal)order.DeliveryLongitude!.Value,
            CurrentLatitude = startLatitude,
            CurrentLongitude = startLongitude,
            Progress = 0m,
            StartedAt = now,
            DurationSeconds = _settings.DefaultDurationSeconds,
            UpdateIntervalSeconds = _settings.UpdateIntervalSeconds,
            Status = DeliveryTrackingStatus.InProgress,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private async Task CancelStartedDeliveryAsync(
        StartDeliveryTrackingResult result,
        CancellationToken cancellationToken)
    {
        if (!result.PreviousStatus.HasValue)
        {
            return;
        }

        var now = DateTime.UtcNow;

        result.Order.Status = result.PreviousStatus.Value;
        result.Order.UpdatedAt = now;
        result.TrackingSession.Status = DeliveryTrackingStatus.Cancelled;
        result.TrackingSession.UpdatedAt = now;

        await _ordersRepository.UpdateAsync(result.Order, cancellationToken);
        await _trackingSessionsRepository.UpdateAsync(
            result.TrackingSession,
            cancellationToken);
        await _ordersRepository.SaveChangesAsync(cancellationToken);
    }

    private sealed record StartDeliveryTrackingResult(
        OrderEntity Order,
        DeliveryTrackingSession TrackingSession,
        bool ShouldEnqueueSimulationJob,
        OrderStatus? PreviousStatus);
}
