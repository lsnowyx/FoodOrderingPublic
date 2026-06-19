namespace Application.DTOs.MenuItemIngredient;

public sealed record MenuItemIngredientResponse(
    Guid Id,
    Guid IngredientId,
    string IngredientName,
    string BaseUnit,
    decimal UnitCost,
    string? AllergenInfo,
    int? CaloriesPerUnit,
    decimal Quantity,
    decimal LineCost,
    decimal LineCalories
);
