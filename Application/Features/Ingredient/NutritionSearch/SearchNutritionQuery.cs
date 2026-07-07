using Application.DTOs.Ingredient;
using MediatR;

namespace Application.Features.Ingredient.NutritionSearch;

public sealed class SearchNutritionQuery : IRequest<IReadOnlyList<NutritionLookupResult>>
{
    public string Query { get; init; } = string.Empty;

    public int PageSize { get; init; }
}
