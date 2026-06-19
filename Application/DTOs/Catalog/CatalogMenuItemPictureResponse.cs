namespace Application.DTOs.Catalog;

public sealed record CatalogMenuItemPictureResponse(
    Guid Id,
    string ImageUrl,
    string? Caption);
