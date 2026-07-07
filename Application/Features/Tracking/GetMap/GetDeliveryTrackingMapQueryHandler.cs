using Application.Abstractions.Services;
using Application.DTOs.DeliveryTracking;
using MediatR;

namespace Application.Features.Tracking.GetMap;

public sealed class GetDeliveryTrackingMapQueryHandler
    : IRequestHandler<GetDeliveryTrackingMapQuery, DeliveryTrackingMapResponse>
{
    private readonly IDeliveryTrackingMapService _deliveryTrackingMapService;

    public GetDeliveryTrackingMapQueryHandler(
        IDeliveryTrackingMapService deliveryTrackingMapService)
    {
        _deliveryTrackingMapService = deliveryTrackingMapService;
    }

    public Task<DeliveryTrackingMapResponse> Handle(
        GetDeliveryTrackingMapQuery request,
        CancellationToken cancellationToken)
    {
        return _deliveryTrackingMapService.GetGuestMapAsync(
            request.Token,
            cancellationToken);
    }
}
