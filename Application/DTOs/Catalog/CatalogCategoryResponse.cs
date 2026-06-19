namespace Application.DTOs.Catalog;

public sealed record CatalogCategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl);
