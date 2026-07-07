using Application.Abstractions.Services;
using Application.DTOs.DeliveryTracking;
using Common.Constants;
using Microsoft.AspNetCore.SignalR;
using WebApi.Hubs;

namespace WebApi.Services;

public sealed class DeliveryTrackingBroadcaster : IDeliveryTrackingBroadcaster
{
    private const string CourierLocationUpdatedEvent = "CourierLocationUpdated";

    private readonly IHubContext<DeliveryTrackingHub> _hubContext;

    public DeliveryTrackingBroadcaster(IHubContext<DeliveryTrackingHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task BroadcastLocationUpdatedAsync(
        DeliveryTrackingLocationUpdate update,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(DeliveryTrackingGroups.ForOrder(update.OrderId))
            .SendAsync(CourierLocationUpdatedEvent, update, cancellationToken);
    }
}
