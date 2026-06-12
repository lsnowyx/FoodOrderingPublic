using Application.Abstractions.Repositories;
using Application.DTOs.MenuItemIngredient;
using MediatR;

namespace Application.Features.MenuItemIngredient.Create;

public class CreateMenuItemIngredientCommandHandler : IRequestHandler<CreateMenuItemIngredientCommand, MenuItemIngredientResponse>
{
    private readonly IMenuItemIngredientsRepository _repo;
    private readonly Application.Abstractions.Repositories.IMenuItemsRepository _menuRepo;
    private readonly Application.Abstractions.Repositories.IIngredientsRepository _ingredientRepo;

    public CreateMenuItemIngredientCommandHandler(IMenuItemIngredientsRepository repo, Application.Abstractions.Repositories.IMenuItemsRepository menuRepo, Application.Abstractions.Repositories.IIngredientsRepository ingredientRepo)
    {
        _repo = repo;
        _menuRepo = menuRepo;
        _ingredientRepo = ingredientRepo;
    }

    public async Task<MenuItemIngredientResponse> Handle(CreateMenuItemIngredientCommand request, CancellationToken cancellationToken)
    {
        // validate menu exists
        var menu = await _menuRepo.GetByIdAsync(request.MenuItemId, cancellationToken);
        if (menu == null) throw new KeyNotFoundException("Menu item not found");

        // validate ingredient exists
        var ingredient = await _ingredientRepo.GetByIdAsync(request.IngredientId, cancellationToken);
        if (ingredient == null) throw new KeyNotFoundException("Ingredient not found");

        // prevent duplicate
        var exists = await _repo.ExistsByMenuItemAndIngredientAsync(request.MenuItemId, request.IngredientId, cancellationToken);
        if (exists) throw new InvalidOperationException("Ingredient already assigned to menu item");

        var toAdd = new Domain.Entities.MenuItemIngredient
        {
            MenuItemId = request.MenuItemId,
            IngredientId = request.IngredientId,
            Quantity = request.Quantity
        };

        await _repo.AddAsync(toAdd, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return new MenuItemIngredientResponse(toAdd.Id, ingredient.Id, ingredient.Name ?? string.Empty, ingredient.AllergenInfo ?? string.Empty, ingredient.CaloriesPerUnit ?? 0, toAdd.Quantity);
    }
}
