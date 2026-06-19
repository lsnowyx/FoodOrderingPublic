using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class MenuItemIngredientsRepository : IMenuItemIngredientsRepository
{
    private readonly AppDbContext _context;

    public MenuItemIngredientsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MenuItemIngredient> AddAsync(MenuItemIngredient entity, CancellationToken cancellationToken = default)
    {
        await _context.MenuItemIngredients.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task DeleteAsync(MenuItemIngredient entity, CancellationToken cancellationToken = default)
    {
        _context.MenuItemIngredients.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<MenuItemIngredient>> GetByMenuItemIdAsync(Guid menuItemId, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItemIngredients
            .Include(mi => mi.Ingredient)
            .Where(mi => mi.MenuItemId == menuItemId)
            .ToListAsync(cancellationToken);
    }

    public async Task<MenuItemIngredient?> GetByMenuItemAndIngredientAsync(Guid menuItemId, Guid ingredientId, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItemIngredients
            .Include(mi => mi.Ingredient)
            .FirstOrDefaultAsync(mi => mi.MenuItemId == menuItemId && mi.IngredientId == ingredientId, cancellationToken);
    }

    public async Task<bool> ExistsByMenuItemAndIngredientAsync(Guid menuItemId, Guid ingredientId, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItemIngredients.AnyAsync(mi => mi.MenuItemId == menuItemId && mi.IngredientId == ingredientId, cancellationToken);
    }

    public Task UpdateAsync(MenuItemIngredient entity, CancellationToken cancellationToken = default)
    {
        _context.MenuItemIngredients.Update(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
