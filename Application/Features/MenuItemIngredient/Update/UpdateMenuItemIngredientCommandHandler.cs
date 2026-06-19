using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.MenuItemIngredient;
using Mapster;
using MediatR;

namespace Application.Features.MenuItemIngredient.Update;

public class UpdateMenuItemIngredientCommandHandler : IRequestHandler<UpdateMenuItemIngredientCommand, MenuItemIngredientResponse>
{
    private readonly IMenuItemIngredientsRepository _repo;
    private readonly IMenuItemsRepository _menuRepo;
    private readonly IMenuItemCostService _menuItemCostService;
    private readonly IApplicationTransaction _transaction;

    public UpdateMenuItemIngredientCommandHandler(
        IMenuItemIngredientsRepository repo,
        IMenuItemsRepository menuRepo,
        IMenuItemCostService menuItemCostService,
        IApplicationTransaction transaction)
    {
        _repo = repo;
        _menuRepo = menuRepo;
        _menuItemCostService = menuItemCostService;
        _transaction = transaction;
    }

    public async Task<MenuItemIngredientResponse> Handle(UpdateMenuItemIngredientCommand request, CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAsync(async transactionCancellationToken =>
        {
            var menu = await _menuRepo.GetByIdAsync(
                request.MenuItemId,
                transactionCancellationToken);
            if (menu == null) throw new KeyNotFoundException("Menu item not found");

            var entity = await _repo.GetByMenuItemAndIngredientAsync(
                request.MenuItemId,
                request.IngredientId,
                transactionCancellationToken);
            if (entity == null) throw new KeyNotFoundException("Menu item ingredient not found");

            entity.Quantity = request.Quantity;

            await _repo.UpdateAsync(entity, transactionCancellationToken);
            await _repo.SaveChangesAsync(transactionCancellationToken);
            await _menuItemCostService.RecalculateMenuItemCostAsync(
                request.MenuItemId,
                transactionCancellationToken);

            return entity.Adapt<MenuItemIngredientResponse>();
        }, cancellationToken);
    }
}
