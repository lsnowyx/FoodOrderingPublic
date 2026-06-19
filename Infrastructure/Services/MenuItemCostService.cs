using Application.Abstractions.Services;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Infrastructure.Services;

public sealed class MenuItemCostService : IMenuItemCostService
{
    private readonly AppDbContext _context;

    public MenuItemCostService(AppDbContext context)
    {
        _context = context;
    }

    public async Task RecalculateMenuItemCostAsync(
        Guid menuItemId,
        CancellationToken cancellationToken = default)
    {
        var menuItemExists = await _context.MenuItems
            .AnyAsync(menuItem => menuItem.Id == menuItemId, cancellationToken);

        if (!menuItemExists)
        {
            throw new KeyNotFoundException("Menu item not found.");
        }

        var restaurantCost = await _context.MenuItemIngredients
            .AsNoTracking()
            .Where(mapping => mapping.MenuItemId == menuItemId)
            .SumAsync(
                mapping => (decimal?)(mapping.Ingredient.UnitCost * mapping.Quantity),
                cancellationToken)
            ?? 0m;

        var totalCalories = await _context.MenuItemIngredients
            .AsNoTracking()
            .Where(mapping => mapping.MenuItemId == menuItemId)
            .SumAsync(
                mapping => (decimal?)(
                    (mapping.Ingredient.CaloriesPerUnit ?? 0)
                    * mapping.Quantity),
                cancellationToken)
            ?? 0m;

        await _context.MenuItems
            .Where(menuItem => menuItem.Id == menuItemId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        menuItem => menuItem.RestaurantCost,
                        restaurantCost)
                    .SetProperty(
                        menuItem => menuItem.TotalCalories,
                        totalCalories),
                cancellationToken);
    }

    public async Task RecalculateMenuItemsUsingIngredientAsync(
        Guid ingredientId,
        CancellationToken cancellationToken = default)
    {
        var menuItemIds = await _context.MenuItemIngredients
            .AsNoTracking()
            .Where(mapping => mapping.IngredientId == ingredientId)
            .Select(mapping => mapping.MenuItemId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var menuItemId in menuItemIds)
        {
            await RecalculateMenuItemCostAsync(menuItemId, cancellationToken);
        }
    }
}
