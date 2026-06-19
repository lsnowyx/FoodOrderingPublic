namespace Application.DTOs.Ingredient;

public sealed record IngredientResponse(
    Guid Id,
    string Name,
    string BaseUnit,
    decimal UnitCost,
    string? AllergenInfo,
    int? CaloriesPerUnit);
