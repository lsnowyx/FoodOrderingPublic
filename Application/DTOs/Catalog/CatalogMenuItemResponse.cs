namespace Application.DTOs.Catalog;

public sealed record CatalogMenuItemResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    decimal TotalCalories,
    Guid CategoryId,
    string? DisplayPictureUrl);
