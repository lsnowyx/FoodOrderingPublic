using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class CategoriesRepository : ICategoriesRepository
{
    private readonly AppDbContext _context;

    public CategoriesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
        return category;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return await GetCategoryQuery()
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetPagedAsync(
        int skip,
        int take,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        return await ApplySearch(GetCategoryQuery(), searchTerm)
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await GetCategoryQuery().CountAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string? searchTerm, CancellationToken cancellationToken = default)
    {
        return await ApplySearch(GetCategoryQuery(), searchTerm).CountAsync(cancellationToken);
    }

    public Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        _context.Categories.Update(category);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Categories.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _context.Categories.Remove(entity);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Category> GetCategoryQuery()
    {
        return _context.Categories.AsNoTracking();
    }

    private static IQueryable<Category> ApplySearch(IQueryable<Category> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return query;
        }

        var trimmedSearchTerm = searchTerm.Trim();

        return query.Where(category =>
            category.Name.Contains(trimmedSearchTerm)
            || (category.Description != null && category.Description.Contains(trimmedSearchTerm)));
    }
}
