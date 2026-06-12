namespace Application.DTOs.MenuItemPicture;

public sealed record MenuItemPictureResponse(Guid Id, Guid MenuItemId, string ImageUrl, string? Caption);
