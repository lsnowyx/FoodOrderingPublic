using Application.Abstractions.Services;
using Application.DTOs.Tracking;
using Mapster;
using MediatR;

namespace Application.Features.Tracking.Get;

public class GetOrderTrackingQueryHandler : IRequestHandler<GetOrderTrackingQuery, OrderTrackingResponse>
{
    private readonly IDeliveryTrackingAccessService _deliveryTrackingAccessService;

    public GetOrderTrackingQueryHandler(
        IDeliveryTrackingAccessService deliveryTrackingAccessService)
    {
        _deliveryTrackingAccessService = deliveryTrackingAccessService;
    }

    public async Task<OrderTrackingResponse> Handle(
        GetOrderTrackingQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _deliveryTrackingAccessService.GetGuestTrackedOrderAsync(
            request.Token,
            cancellationToken);

        return order.Adapt<OrderTrackingResponse>();
    }
}
