namespace Application.DTOs.Catalog;

public sealed record CatalogMenuItemDetailsResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    decimal TotalCalories,
    CatalogCategorySummaryResponse Category,
    IReadOnlyList<CatalogMenuItemPictureResponse> Pictures,
    IReadOnlyList<CatalogMenuItemIngredientResponse> Ingredients);
