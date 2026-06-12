using Application.Abstractions.Repositories;
using Application.DTOs.MenuItemIngredient;
using MediatR;

namespace Application.Features.MenuItemIngredient.Update;

public class UpdateMenuItemIngredientCommandHandler : IRequestHandler<UpdateMenuItemIngredientCommand, MenuItemIngredientResponse>
{
    private readonly IMenuItemIngredientsRepository _repo;
    private readonly Application.Abstractions.Repositories.IMenuItemsRepository _menuRepo;

    public UpdateMenuItemIngredientCommandHandler(IMenuItemIngredientsRepository repo, Application.Abstractions.Repositories.IMenuItemsRepository menuRepo)
    {
        _repo = repo;
        _menuRepo = menuRepo;
    }

    public async Task<MenuItemIngredientResponse> Handle(UpdateMenuItemIngredientCommand request, CancellationToken cancellationToken)
    {
        var menu = await _menuRepo.GetByIdAsync(request.MenuItemId, cancellationToken);
        if (menu == null) throw new KeyNotFoundException("Menu item not found");

        var entity = await _repo.GetByMenuItemAndIngredientAsync(request.MenuItemId, request.IngredientId, cancellationToken);
        if (entity == null) throw new KeyNotFoundException("Menu item ingredient not found");

        entity.Quantity = request.Quantity;

        await _repo.UpdateAsync(entity, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return new MenuItemIngredientResponse(
            entity.Id,
            entity.IngredientId,
            entity.Ingredient?.Name ?? string.Empty,
            entity.Ingredient?.AllergenInfo,
            entity.Ingredient?.CaloriesPerUnit,
            entity.Quantity ?? string.Empty);
    }
}
