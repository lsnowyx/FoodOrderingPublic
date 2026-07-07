using Application.DTOs.Order;
using Application.Features.Order;
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
            .Map(dest => dest.UserId, _ => (Guid?)null)
            .Map(dest => dest.PayOnline, src => src.PayOnline);

        config.NewConfig<CreateOrderItemDto, CreateOrderItemDto>();
        config.NewConfig<AdjustItemDto, AdjustItemDto>();

        config.NewConfig<OrderItem, OrderItemDto>()
            .Map(dest => dest.MenuItemName, src => src.MenuItem != null ? src.MenuItem.Name : string.Empty);

        config.NewConfig<OrderItem, CustomerOrderItemResponse>()
            .Map(dest => dest.MenuItemName, src => src.MenuItem != null ? src.MenuItem.Name : string.Empty)
            .Map(dest => dest.LineTotal, src => src.UnitPrice * src.Quantity)
            .Map(
                dest => dest.TotalCalories,
                src => src.MenuItem != null
                    ? src.MenuItem.TotalCalories * src.Quantity
                    : 0m);

        config.NewConfig<Order, OrderResponse>()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.PaymentMethod, src => src.PaymentMethod.ToString())
            .Map(dest => dest.PaymentStatus, src => src.PaymentStatus.ToString())
            .Map(dest => dest.Items, src => src.OrderItems);

        config.NewConfig<Order, CreateOrderResponse>()
            .Map(dest => dest.OrderId, src => src.Id)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.PaymentMethod, src => src.PaymentMethod.ToString())
            .Map(dest => dest.PaymentStatus, src => src.PaymentStatus.ToString())
            .Map(dest => dest.PaymentUrl, _ => (string?)null);

        config.NewConfig<Order, CustomerOrderSummaryResponse>()
            .Map(dest => dest.OrderId, src => src.Id)
            .Map(dest => dest.CreatedAt, src => src.OrderDate)
            .Map(dest => dest.OrderStatus, src => src.Status.ToString())
            .Map(dest => dest.PaymentMethod, src => src.PaymentMethod.ToString())
            .Map(dest => dest.PaymentStatus, src => OrderPaymentDisplay.GetEffectivePaymentStatus(src).ToString())
            .Map(dest => dest.ItemsCount, src => src.OrderItems.Sum(orderItem => orderItem.Quantity))
            .Map(dest => dest.CanPayOnlineAgain, src => OrderPaymentDisplay.CanPayOnlineAgain(src))
            .Map(dest => dest.PaymentMessage, src => OrderPaymentDisplay.GetPaymentMessage(src));

        config.NewConfig<Order, CustomerOrderDetailsResponse>()
            .Map(dest => dest.OrderId, src => src.Id)
            .Map(dest => dest.CreatedAt, src => src.OrderDate)
            .Map(dest => dest.OrderStatus, src => src.Status.ToString())
            .Map(dest => dest.PaymentMethod, src => src.PaymentMethod.ToString())
            .Map(dest => dest.PaymentStatus, src => OrderPaymentDisplay.GetEffectivePaymentStatus(src).ToString())
            .Map(dest => dest.CanPayOnlineAgain, src => OrderPaymentDisplay.CanPayOnlineAgain(src))
            .Map(dest => dest.PaymentMessage, src => OrderPaymentDisplay.GetPaymentMessage(src))
            .Map(dest => dest.Items, src => src.OrderItems);
    }
}
