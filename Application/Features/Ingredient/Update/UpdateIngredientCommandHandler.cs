using Application.Abstractions.Persistence;
using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Caching;
using Application.DTOs.Ingredient;
using Mapster;
using MediatR;

namespace Application.Features.Ingredient.Update;

public class UpdateIngredientCommandHandler : IRequestHandler<UpdateIngredientCommand, IngredientResponse>
{
    private readonly IIngredientsRepository _repo;
    private readonly IMenuItemCostService _menuItemCostService;
    private readonly IApplicationTransaction _transaction;
    private readonly ICacheService _cacheService;

    public UpdateIngredientCommandHandler(
        IIngredientsRepository repo,
        IMenuItemCostService menuItemCostService,
        IApplicationTransaction transaction,
        ICacheService cacheService)
    {
        _repo = repo;
        _menuItemCostService = menuItemCostService;
        _transaction = transaction;
        _cacheService = cacheService;
    }

    public async Task<IngredientResponse> Handle(UpdateIngredientCommand request, CancellationToken cancellationToken)
    {
        return await _transaction.ExecuteAsync(async transactionCancellationToken =>
        {
            var existing = await _repo.GetByIdAsync(
                request.Id,
                transactionCancellationToken);
            if (existing == null) throw new KeyNotFoundException("Ingredient not found");

            existing.Name = request.Name;
            existing.BaseUnit = request.BaseUnit.Trim();
            existing.UnitCost = request.UnitCost;
            existing.AllergenInfo = request.AllergenInfo;
            existing.CaloriesPerUnit = request.CaloriesPerUnit;

            await _repo.UpdateAsync(existing, transactionCancellationToken);
            await _repo.SaveChangesAsync(transactionCancellationToken);
            await _menuItemCostService.RecalculateMenuItemsUsingIngredientAsync(
                existing.Id,
                transactionCancellationToken);
            await CacheInvalidationHelper.InvalidateIngredientCachesAsync(
                _cacheService,
                transactionCancellationToken);

            return existing.Adapt<IngredientResponse>();
        }, cancellationToken);
    }
}
