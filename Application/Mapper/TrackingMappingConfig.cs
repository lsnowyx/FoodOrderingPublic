using Application.DTOs.Tracking;
using Application.Features.Tracking.Get;
using Domain.Entities;
using Mapster;

namespace Application.Mapper;

public class TrackingMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<GetOrderTrackingRequest, GetOrderTrackingQuery>();

        config.NewConfig<Order, OrderTrackingResponse>()
            .Map(dest => dest.OrderId, src => src.Id)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.DeliveryStatus, src => src.Status.ToString())
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt)
            .Map(dest => dest.DeliveredAt, src => src.DeliveredAt);
    }
}
