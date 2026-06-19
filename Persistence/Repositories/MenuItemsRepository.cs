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

    public async Task<IReadOnlyList<MenuItem>> GetPagedAsync(
        int skip,
        int take,
        string? searchTerm,
        Guid? categoryId,
        bool? isAvailable,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(GetMenuItemQuery(), searchTerm, categoryId, isAvailable)
            .OrderBy(m => m.Name)
            .ThenBy(m => m.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MenuItem>> GetAvailableByCategoryPagedAsync(
        Guid categoryId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .Include(m => m.MenuItemPictures)
            .Where(m => m.CategoryId == categoryId && m.IsAvailable)
            .OrderBy(m => m.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? searchTerm,
        Guid? categoryId,
        bool? isAvailable,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(GetMenuItemQuery(), searchTerm, categoryId, isAvailable)
            .CountAsync(cancellationToken);
    }

    public async Task<int> CountAvailableByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItems
            .CountAsync(m => m.CategoryId == categoryId && m.IsAvailable, cancellationToken);
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

    private IQueryable<MenuItem> GetMenuItemQuery()
    {
        return _context.MenuItems
            .AsNoTracking()
            .Include(menuItem => menuItem.Category);
    }

    private static IQueryable<MenuItem> ApplyFilters(
        IQueryable<MenuItem> query,
        string? searchTerm,
        Guid? categoryId,
        bool? isAvailable)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var trimmedSearchTerm = searchTerm.Trim();

            query = query.Where(menuItem =>
                menuItem.Name.Contains(trimmedSearchTerm)
                || (menuItem.Description != null && menuItem.Description.Contains(trimmedSearchTerm)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(menuItem => menuItem.CategoryId == categoryId.Value);
        }

        if (isAvailable.HasValue)
        {
            query = query.Where(menuItem => menuItem.IsAvailable == isAvailable.Value);
        }

        return query;
    }
}
