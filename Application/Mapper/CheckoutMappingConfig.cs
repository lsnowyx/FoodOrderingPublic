using Application.DTOs.Checkout;
using Application.Features.Checkout.Guest;
using Domain.Entities;
using Mapster;

namespace Application.Mapper;

public class CheckoutMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<GuestCheckoutItemRequest, GuestCheckoutItemCommand>();
        config.NewConfig<GuestCheckoutRequest, GuestCheckoutCommand>();

        config.NewConfig<Order, GuestCheckoutResponse>()
            .Map(dest => dest.OrderId, src => src.Id)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.CreatedAt, src => src.OrderDate)
            .Map(dest => dest.Total, src => src.OrderItems.Sum(orderItem => orderItem.UnitPrice * orderItem.Quantity))
            // Raw tracking tokens are returned once by the handler and are never sourced from the entity.
            .Ignore(dest => dest.TrackingToken);
    }
}
