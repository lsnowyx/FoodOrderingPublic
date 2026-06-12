using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class MenuItemsRepository : IMenuItemsRepository
{
    private readonly AppDbContext _context;

    public MenuItemsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MenuItem> AddAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        await _context.MenuItems.AddAsync(menuItem, cancellationToken);
        return menuItem;
    }

    public async Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .Include(m => m.MenuItemIngredients)
                .ThenInclude(mi => mi.Ingredient)
            .Include(m => m.MenuItemPictures)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<MenuItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems.Include(m => m.Category).ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        _context.MenuItems.Update(menuItem);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.MenuItems.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _context.MenuItems.Remove(entity);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
