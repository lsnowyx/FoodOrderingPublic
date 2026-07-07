using Application.Caching;
using Application.DTOs.MenuItem;
using Application.DTOs.Common;

namespace Application.Features.MenuItem.Get;

public sealed class GetMenuItemsQuery : ICachedQuery<PaginatedResponse<MenuItemResponse>>
{
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.DefaultPageSize;
    public string? SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
    public bool? IsAvailable { get; init; }

    public string CacheKey => CacheKeyHelper.AdminMenuItems(
        Page,
        PageSize,
        SearchTerm,
        CategoryId,
        IsAvailable);

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}
