namespace Application.DTOs.Ingredient;

public sealed record NutritionLookupResult(
    string ExternalId,
    string Name,
    string? BrandName,
    string? DataType,
    string BaseUnit,
    int? CaloriesPerUnit,
    string Source);
