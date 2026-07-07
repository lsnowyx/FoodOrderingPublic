using Application.Abstractions.Services;
using Application.DTOs.DeliveryTracking;
using MediatR;

namespace Application.Features.Order.GetCustomerDeliveryTrackingMap;

public sealed class GetCustomerDeliveryTrackingMapQueryHandler
    : IRequestHandler<GetCustomerDeliveryTrackingMapQuery, DeliveryTrackingMapResponse?>
{
    private readonly IDeliveryTrackingMapService _deliveryTrackingMapService;

    public GetCustomerDeliveryTrackingMapQueryHandler(
        IDeliveryTrackingMapService deliveryTrackingMapService)
    {
        _deliveryTrackingMapService = deliveryTrackingMapService;
    }

    public Task<DeliveryTrackingMapResponse?> Handle(
        GetCustomerDeliveryTrackingMapQuery request,
        CancellationToken cancellationToken)
    {
        return _deliveryTrackingMapService.GetCustomerMapAsync(
            request.OrderId,
            request.CustomerId,
            cancellationToken);
    }
}
