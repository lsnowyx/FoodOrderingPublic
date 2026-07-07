using Application.DTOs.Ingredient;

namespace Application.Abstractions.Services;

public interface INutritionLookupService
{
    Task<IReadOnlyList<NutritionLookupResult>> SearchAsync(
        string query,
        int pageSize,
        CancellationToken cancellationToken = default);
}
