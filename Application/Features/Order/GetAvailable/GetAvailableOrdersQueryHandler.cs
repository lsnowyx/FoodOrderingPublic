using Application.Abstractions.Repositories;
using Application.DTOs.Common;
using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.GetAvailable;

public class GetAvailableOrdersQueryHandler
    : IRequestHandler<GetAvailableOrdersQuery, PaginatedResponse<OrderResponse>>
{
    private readonly IOrdersRepository _repo;

    public GetAvailableOrdersQueryHandler(IOrdersRepository repo)
    {
        _repo = repo;
    }

    public async Task<PaginatedResponse<OrderResponse>> Handle(
        GetAvailableOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(request.Page, request.PageSize);
        var totalCount = await _repo.CountAvailableAsync(
            request.Status,
            request.IsPaid,
            cancellationToken);
        var orders = await _repo.GetAvailablePagedAsync(
            pagination.Skip,
            pagination.PageSize,
            request.Status,
            request.IsPaid,
            cancellationToken);
        var items = orders.Select(OrderResponseFactory.Create).ToList();

        return PaginatedResponse<OrderResponse>.Create(pagination, totalCount, items);
    }
}
