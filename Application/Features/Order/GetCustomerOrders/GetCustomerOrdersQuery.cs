using Application.DTOs.Common;
using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.GetCustomerOrders;

public sealed class GetCustomerOrdersQuery : IRequest<PaginatedResponse<CustomerOrderSummaryResponse>>
{
    public Guid CustomerId { get; init; }
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.DefaultPageSize;
}
