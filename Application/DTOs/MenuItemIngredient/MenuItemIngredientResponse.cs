namespace Application.DTOs.MenuItemIngredient;

public sealed record MenuItemIngredientResponse(
    Guid Id,
    Guid IngredientId,
    string IngredientName,
    string? AllergenInfo,
    int? CaloriesPerUnit,
    string Quantity
);