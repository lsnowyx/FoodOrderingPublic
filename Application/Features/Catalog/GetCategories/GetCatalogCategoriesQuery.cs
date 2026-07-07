using Application.Caching;
using Application.DTOs.Catalog;
using Application.DTOs.Common;

namespace Application.Features.Catalog.GetCategories;

public sealed class GetCatalogCategoriesQuery : ICachedQuery<PaginatedResponse<CatalogCategoryResponse>>
{
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.DefaultPageSize;

    public string CacheKey => CacheKeyHelper.CatalogCategories(Page, PageSize);

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(10);
}
