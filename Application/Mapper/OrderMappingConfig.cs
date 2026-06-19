using Application.DTOs.Order;
using Application.Features.Order.AdjustItems;
using Application.Features.Order.Create;
using Application.Features.Order.UpdatePayment;
using Application.Features.Order.UpdateStatus;
using Domain.Entities;
using Mapster;

namespace Application.Mapper;

public class OrderMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UpdateOrderStatusRequest, UpdateOrderStatusCommand>()
            // Set from the route and authenticated user claim by OrdersController.
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.OrderManagerId);

        config.NewConfig<UpdateOrderPaymentRequest, UpdateOrderPaymentCommand>()
            // Set from the route and authenticated user claim by OrdersController.
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.OrderManagerId);

        config.NewConfig<AdjustOrderItemsRequest, AdjustOrderItemsCommand>()
            // Set from the route and authenticated user claim by OrdersController.
            .Ignore(dest => dest.OrderId)
            .Map(dest => dest.OrderManagerId, _ => (Guid?)null);

        config.NewConfig<CreateOrderRequest, CreateOrderCommand>()
            // Set from the authenticated user claim by OrdersController.
            .Map(dest => dest.UserId, _ => (Guid?)null);

        config.NewConfig<CreateOrderItemDto, CreateOrderItemDto>();
        config.NewConfig<AdjustItemDto, AdjustItemDto>();

        config.NewConfig<OrderItem, OrderItemDto>()
            .Map(dest => dest.MenuItemName, src => src.MenuItem != null ? src.MenuItem.Name : string.Empty);

        config.NewConfig<Order, OrderResponse>()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Items, src => src.OrderItems);
    }
}
