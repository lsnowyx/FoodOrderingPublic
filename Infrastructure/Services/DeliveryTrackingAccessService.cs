using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Common.Constants;
using Common.Enums;
using Domain.Entities;

namespace Infrastructure.Services;

public sealed class DeliveryTrackingAccessService : IDeliveryTrackingAccessService
{
    private readonly IOrderTrackingLinksRepository _trackingLinksRepository;
    private readonly IOrderTrackingTokenService _trackingTokenService;
    private readonly IOrdersRepository _ordersRepository;

    public DeliveryTrackingAccessService(
        IOrderTrackingLinksRepository trackingLinksRepository,
        IOrderTrackingTokenService trackingTokenService,
        IOrdersRepository ordersRepository)
    {
        _trackingLinksRepository = trackingLinksRepository;
        _trackingTokenService = trackingTokenService;
        _ordersRepository = ordersRepository;
    }

    public async Task<Order> GetGuestTrackedOrderAsync(
        string trackingToken,
        CancellationToken cancellationToken = default)
    {
        var trackingLink = await GetValidTrackingLinkAsync(
            trackingToken,
            cancellationToken);

        return trackingLink.Order;
    }

    public async Task<Order> GetGuestTrackedOrderAsync(
        Guid orderId,
        string trackingToken,
        CancellationToken cancellationToken = default)
    {
        var trackingLink = await GetValidTrackingLinkAsync(
            trackingToken,
            cancellationToken);

        if (trackingLink.OrderId != orderId)
        {
            throw new UnauthorizedAccessException(
                "Tracking token does not belong to this order.");
        }

        return trackingLink.Order;
    }

    public Task<Order?> GetCustomerTrackedOrderAsync(
        Guid orderId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return _ordersRepository.GetCustomerOrderDetailsAsync(
            orderId,
            customerId,
            cancellationToken);
    }

    private async Task<OrderTrackingLink> GetValidTrackingLinkAsync(
        string trackingToken,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var tokenHash = _trackingTokenService.HashToken(trackingToken);
        var trackingLink = await _trackingLinksRepository.GetByTokenHashAsync(
            tokenHash,
            cancellationToken);

        if (trackingLink is null)
        {
            throw new KeyNotFoundException("Tracking token was not found.");
        }

        if (trackingLink.ExpiresAt <= now)
        {
            throw new UnauthorizedAccessException("Tracking token has expired.");
        }

        if (trackingLink.RevokedAt is not null)
        {
            throw new UnauthorizedAccessException("Tracking token has been revoked.");
        }

        if (trackingLink.Scope != TrackingScopeConstants.OrderTrackingRead)
        {
            throw new UnauthorizedAccessException("Tracking token scope is invalid.");
        }

        if (trackingLink.Order is null)
        {
            throw new KeyNotFoundException("Tracked order was not found.");
        }

        if (trackingLink.Order.Status == OrderStatus.Delivered)
        {
            throw new UnauthorizedAccessException("Tracking token has been revoked.");
        }

        trackingLink.LastUsedAt = now;
        await _trackingLinksRepository.UpdateAsync(trackingLink, cancellationToken);
        await _trackingLinksRepository.SaveChangesAsync(cancellationToken);

        return trackingLink;
    }
}
