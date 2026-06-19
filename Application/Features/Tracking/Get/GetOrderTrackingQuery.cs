using Application.DTOs.Tracking;
using MediatR;

namespace Application.Features.Tracking.Get;

public class GetOrderTrackingQuery : IRequest<OrderTrackingResponse>
{
    public string Token { get; set; } = null!;
}
