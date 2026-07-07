using Application.Abstractions.Repositories;
using Application.DTOs.Common;
using Application.DTOs.Order;
using Mapster;
using MediatR;

namespace Application.Features.Order.GetCustomerOrders;

public sealed class GetCustomerOrdersQueryHandler
    : IRequestHandler<GetCustomerOrdersQuery, PaginatedResponse<CustomerOrderSummaryResponse>>
{
    private readonly IOrdersRepository _ordersRepository;

    public GetCustomerOrdersQueryHandler(IOrdersRepository ordersRepository)
    {
        _ordersRepository = ordersRepository;
    }

    public async Task<PaginatedResponse<CustomerOrderSummaryResponse>> Handle(
        GetCustomerOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(request.Page, request.PageSize);
        var totalCount = await _ordersRepository.CountOrdersByCustomerIdAsync(
            request.CustomerId,
            cancellationToken);
        var orders = await _ordersRepository.GetOrdersByCustomerIdPagedAsync(
            request.CustomerId,
            pagination.Skip,
            pagination.PageSize,
            cancellationToken);
        var items = orders.Adapt<List<CustomerOrderSummaryResponse>>();

        return PaginatedResponse<CustomerOrderSummaryResponse>.Create(
            pagination,
            totalCount,
            items);
    }
}
