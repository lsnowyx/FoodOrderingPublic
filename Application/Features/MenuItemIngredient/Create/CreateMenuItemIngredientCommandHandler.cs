using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.MenuItemIngredient;
using Mapster;
using MediatR;

namespace Application.Features.MenuItemIngredient.Create;

public class CreateMenuItemIngredientCommandHandler : IRequestHandler<CreateMenuItemIngredientCommand, MenuItemIngredientResponse>
{
    private readonly IMenuItemIngredientsRepository _repo;
    private readonly IMenuItemsRepository _menuRepo;
    private readonly IIngredientsRepository _ingredientRepo;
    private readonly IMenuItemCostService _menuItemCostService;
    private readonly IApplicationTransaction _transaction;

    public CreateMenuItemIngredientCommandHandler(
        IMenuItemIngredientsRepository repo,
        IMenuItemsRepository menuRepo,
        IIngredientsRepository ingredientRepo,
        IMenuItemCostService menuItemCostService,
        IApplicationTransaction transaction)
    {
        _repo = repo;
        _menuRepo = menuRepo;
        _ingredientRepo = ingredientRepo;
        _menuItemCostService = menuItemCostService;
        _transaction = transaction;
    }

    public async Task<MenuItemIngredientResponse> Handle(CreateMenuItemIngredientCommand request, CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAsync(async transactionCancellationToken =>
        {
            var menu = await _menuRepo.GetByIdAsync(
                request.MenuItemId,
                transactionCancellationToken);
            if (menu == null) throw new KeyNotFoundException("Menu item not found");

            var ingredient = await _ingredientRepo.GetByIdAsync(
                request.IngredientId,
                transactionCancellationToken);
            if (ingredient == null) throw new KeyNotFoundException("Ingredient not found");

            var exists = await _repo.ExistsByMenuItemAndIngredientAsync(
                request.MenuItemId,
                request.IngredientId,
                transactionCancellationToken);
            if (exists)
                throw new InvalidOperationException("Ingredient already assigned to menu item");

            var toAdd = new Domain.Entities.MenuItemIngredient
            {
                MenuItemId = request.MenuItemId,
                IngredientId = request.IngredientId,
                Ingredient = ingredient,
                Quantity = request.Quantity
            };

            await _repo.AddAsync(toAdd, transactionCancellationToken);
            await _repo.SaveChangesAsync(transactionCancellationToken);
            await _menuItemCostService.RecalculateMenuItemCostAsync(
                request.MenuItemId,
                transactionCancellationToken);

            return toAdd.Adapt<MenuItemIngredientResponse>();
        }, cancellationToken);
    }
}
