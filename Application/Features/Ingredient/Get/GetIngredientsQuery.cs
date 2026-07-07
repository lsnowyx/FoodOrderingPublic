using Application.Caching;
using Application.DTOs.Ingredient;
using Application.DTOs.Common;

namespace Application.Features.Ingredient.Get;

public sealed class GetIngredientsQuery : ICachedQuery<PaginatedResponse<IngredientResponse>>
{
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.DefaultPageSize;
    public string? SearchTerm { get; init; }

    public string CacheKey => CacheKeyHelper.AdminIngredients(
        Page,
        PageSize,
        SearchTerm);

    public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
}
