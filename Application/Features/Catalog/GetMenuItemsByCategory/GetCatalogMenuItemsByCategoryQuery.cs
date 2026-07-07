using Application.Caching;
using Application.DTOs.Catalog;
using Application.DTOs.Common;

namespace Application.Features.Catalog.GetMenuItemsByCategory;

public sealed class GetCatalogMenuItemsByCategoryQuery : ICachedQuery<PaginatedResponse<CatalogMenuItemResponse>>
{
    public Guid CategoryId { get; init; }
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.DefaultPageSize;

    public string CacheKey => CacheKeyHelper.CatalogMenuItemsByCategory(
        CategoryId,
        Page,
        PageSize);

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}
