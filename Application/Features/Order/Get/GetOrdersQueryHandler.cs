using Application.Abstractions.Repositories;
using Application.DTOs.Common;
using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.Get;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PaginatedResponse<OrderResponse>>
{
    private readonly IOrdersRepository _repo;

    public GetOrdersQueryHandler(IOrdersRepository repo)
    {
        _repo = repo;
    }

    public async Task<PaginatedResponse<OrderResponse>> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(request.Page, request.PageSize);
        var totalCount = await _repo.CountAsync(request.Status, request.IsPaid, cancellationToken);
        var orders = await _repo.GetPagedAsync(
            pagination.Skip,
            pagination.PageSize,
            request.Status,
            request.IsPaid,
            cancellationToken);
        var items = orders.Select(OrderResponseFactory.Create).ToList();

        return PaginatedResponse<OrderResponse>.Create(pagination, totalCount, items);
    }
}
