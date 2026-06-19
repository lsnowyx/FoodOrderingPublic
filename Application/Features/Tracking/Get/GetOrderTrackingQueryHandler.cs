using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Tracking;
using Common.Constants;
using Common.Enums;
using Mapster;
using MediatR;

namespace Application.Features.Tracking.Get;

public class GetOrderTrackingQueryHandler : IRequestHandler<GetOrderTrackingQuery, OrderTrackingResponse>
{
    private readonly IOrderTrackingLinksRepository _trackingLinksRepository;
    private readonly IOrderTrackingTokenService _trackingTokenService;

    public GetOrderTrackingQueryHandler(
        IOrderTrackingLinksRepository trackingLinksRepository,
        IOrderTrackingTokenService trackingTokenService)
    {
        _trackingLinksRepository = trackingLinksRepository;
        _trackingTokenService = trackingTokenService;
    }

    public async Task<OrderTrackingResponse> Handle(GetOrderTrackingQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var tokenHash = _trackingTokenService.HashToken(request.Token);
        var trackingLink = await _trackingLinksRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

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

        return trackingLink.Order.Adapt<OrderTrackingResponse>();
    }
}
