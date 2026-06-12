namespace Application.DTOs.Ingredient;

public sealed record IngredientResponse(Guid Id, string Name, string? AllergenInfo, int? CaloriesPerUnit);
