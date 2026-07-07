using Application.DTOs.DeliveryTracking;
using MediatR;

namespace Application.Features.Order.GetCustomerDeliveryTrackingMap;

public sealed class GetCustomerDeliveryTrackingMapQuery
    : IRequest<DeliveryTrackingMapResponse?>
{
    public Guid OrderId { get; set; }

    public Guid CustomerId { get; set; }
}
