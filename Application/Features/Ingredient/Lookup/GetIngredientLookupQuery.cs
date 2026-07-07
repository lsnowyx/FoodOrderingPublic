using Application.Caching;
using Application.DTOs.Common;
using Application.DTOs.Ingredient;

namespace Application.Features.Ingredient.Lookup;

public sealed class GetIngredientLookupQuery : ICachedQuery<PaginatedResponse<IngredientLookupResponse>>
{
    public string? SearchTerm { get; init; }
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.LookupPageSize;

    public string CacheKey => CacheKeyHelper.IngredientLookup(
        SearchTerm,
        Page,
        PageSize);

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(20);
}
