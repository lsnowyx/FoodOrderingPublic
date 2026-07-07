using Application.Caching;
using Application.DTOs.Category;
using Application.DTOs.Common;

namespace Application.Features.Category.Lookup;

public sealed class GetCategoryLookupQuery : ICachedQuery<PaginatedResponse<CategoryLookupResponse>>
{
    public string? SearchTerm { get; init; }
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.LookupPageSize;

    public string CacheKey => CacheKeyHelper.CategoryLookup(
        SearchTerm,
        Page,
        PageSize);

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(20);
}
