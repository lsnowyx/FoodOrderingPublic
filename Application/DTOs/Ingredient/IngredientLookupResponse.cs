namespace Application.DTOs.Ingredient;

public sealed record IngredientLookupResponse(
    Guid Id,
    string Text,
    string BaseUnit,
    decimal UnitCost,
    string? AllergenInfo,
    int? CaloriesPerUnit);
