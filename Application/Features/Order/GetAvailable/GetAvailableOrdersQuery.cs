using Application.DTOs.Common;
using Application.DTOs.Order;
using Common.Enums;
using MediatR;

namespace Application.Features.Order.GetAvailable;

public sealed class GetAvailableOrdersQuery : IRequest<PaginatedResponse<OrderResponse>>
{
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.DefaultPageSize;
    public OrderStatus? Status { get; init; }
    public bool? IsPaid { get; init; }
}
