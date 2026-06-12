using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class IngredientsRepository : IIngredientsRepository
{
    private readonly AppDbContext _context;

    public IngredientsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Ingredient> AddAsync(Ingredient ingredient, CancellationToken cancellationToken = default)
    {
        await _context.Ingredients.AddAsync(ingredient, cancellationToken);
        return ingredient;
    }

    public async Task<Ingredient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Ingredients.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Ingredient>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Ingredients.ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(Ingredient ingredient, CancellationToken cancellationToken = default)
    {
        _context.Ingredients.Update(ingredient);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Ingredients.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _context.Ingredients.Remove(entity);
        }
    }

    public async Task<bool> IsUsedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItemIngredients.AnyAsync(mi => mi.IngredientId == id, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
