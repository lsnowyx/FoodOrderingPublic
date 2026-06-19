using AdminPanel.Models.Common;
using AdminPanel.Models.Order;
using Application.DTOs.Common;
using Application.DTOs.Order;
using Mapster;

namespace AdminPanel.Mapper;

public sealed class OrderMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<OrderItemDto, OrderItemViewModel>();
        config.NewConfig<OrderResponse, OrderViewModel>();
        config.NewConfig<PaginatedResponse<OrderResponse>, PaginatedViewModel<OrderViewModel>>()
            .Map(destination => destination.Page, source => source.Page)
            .Map(destination => destination.PageSize, source => source.PageSize)
            .Map(destination => destination.TotalCount, source => source.TotalCount)
            .Map(destination => destination.TotalPages, source => source.TotalPages)
            .Map(destination => destination.Items, source => source.Items);

        config.NewConfig<UpdateOrderPaymentViewModel, UpdateOrderPaymentRequest>();
        config.NewConfig<UpdateOrderStatusViewModel, UpdateOrderStatusRequest>();
    }
}
