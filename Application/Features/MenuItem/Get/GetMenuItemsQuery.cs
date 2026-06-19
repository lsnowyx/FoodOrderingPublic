using Application.DTOs.MenuItem;
using Application.DTOs.Common;
using MediatR;

namespace Application.Features.MenuItem.Get;

public sealed class GetMenuItemsQuery : IRequest<PaginatedResponse<MenuItemResponse>>
{
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.DefaultPageSize;
    public string? SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
    public bool? IsAvailable { get; init; }
}
