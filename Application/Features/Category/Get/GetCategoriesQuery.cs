using Application.Caching;
using Application.DTOs.Category;
using Application.DTOs.Common;

namespace Application.Features.Category.Get;

public sealed class GetCategoriesQuery : ICachedQuery<PaginatedResponse<CategoryResponse>>
{
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.DefaultPageSize;
    public string? SearchTerm { get; init; }

    public string CacheKey => CacheKeyHelper.AdminCategories(
        Page,
        PageSize,
        SearchTerm);

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}
