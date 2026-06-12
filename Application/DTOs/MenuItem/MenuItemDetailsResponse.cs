using Application.DTOs.MenuItemIngredient;
using Application.DTOs.MenuItemPicture;

namespace Application.DTOs.MenuItem;

public sealed record MenuItemDetailsResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    Guid CategoryId,
    string CategoryName,
    bool IsAvailable,
    IEnumerable<MenuItemIngredientResponse> Ingredients,
    IEnumerable<MenuItemPictureResponse> Pictures
);
