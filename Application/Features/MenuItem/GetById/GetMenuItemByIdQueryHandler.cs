using Application.Abstractions.Repositories;
using Application.DTOs.MenuItem;
using Application.DTOs.MenuItemIngredient;
using Application.DTOs.MenuItemPicture;
using MediatR;

namespace Application.Features.MenuItem.GetById;

public class GetMenuItemByIdQueryHandler : IRequestHandler<GetMenuItemByIdQuery, MenuItemDetailsResponse?>
{
    private readonly IMenuItemsRepository _repo;

    public GetMenuItemByIdQueryHandler(IMenuItemsRepository repo)
    {
        _repo = repo;
    }

    public async Task<MenuItemDetailsResponse?> Handle(GetMenuItemByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null) return null;

        var ingredients = entity.MenuItemIngredients.Select(mi => new MenuItemIngredientResponse(mi.Id, mi.IngredientId, mi.Ingredient?.Name ?? string.Empty, mi.Ingredient?.AllergenInfo ?? string.Empty, mi.Ingredient?.CaloriesPerUnit ?? 0, mi.Quantity ?? string.Empty));
        var pictures = entity.MenuItemPictures.Select(p => new MenuItemPictureResponse(p.Id, p.MenuItemId, p.ImageUrl, p.Caption));

        return new MenuItemDetailsResponse(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Price,
            entity.CategoryId,
            entity.Category?.Name ?? string.Empty,
            entity.IsAvailable,
            ingredients,
            pictures
        );
    }
}
