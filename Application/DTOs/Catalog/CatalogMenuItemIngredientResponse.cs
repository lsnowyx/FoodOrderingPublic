namespace Application.DTOs.Catalog;

public sealed record CatalogMenuItemIngredientResponse(
    Guid IngredientId,
    string Name,
    string? AllergenInfo);
