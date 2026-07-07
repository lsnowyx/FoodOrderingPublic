using Application.Abstractions.Services;
using Application.DTOs.Ingredient;
using MediatR;

namespace Application.Features.Ingredient.NutritionSearch;

public sealed class SearchNutritionQueryHandler
    : IRequestHandler<SearchNutritionQuery, IReadOnlyList<NutritionLookupResult>>
{
    private const int MinimumQueryLength = 2;

    private readonly INutritionLookupService _nutritionLookupService;

    public SearchNutritionQueryHandler(INutritionLookupService nutritionLookupService)
    {
        _nutritionLookupService = nutritionLookupService;
    }

    public async Task<IReadOnlyList<NutritionLookupResult>> Handle(
        SearchNutritionQuery request,
        CancellationToken cancellationToken)
    {
        var query = request.Query.Trim();
        if (query.Length < MinimumQueryLength)
        {
            throw new ArgumentException(
                $"Nutrition search query must contain at least {MinimumQueryLength} characters.",
                nameof(request.Query));
        }

        return await _nutritionLookupService.SearchAsync(
            query,
            request.PageSize,
            cancellationToken);
    }
}
