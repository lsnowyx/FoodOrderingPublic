using Application.DTOs.DeliveryTracking;
using MediatR;

namespace Application.Features.Tracking.GetMap;

public sealed class GetDeliveryTrackingMapQuery : IRequest<DeliveryTrackingMapResponse>
{
    public string Token { get; set; } = null!;
}
