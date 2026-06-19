using Application.Abstractions.Repositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class MenuItemPicturesRepository : IMenuItemPicturesRepository
{
    private readonly AppDbContext _context;

    public MenuItemPicturesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MenuItemPicture> AddAsync(MenuItemPicture entity, CancellationToken cancellationToken = default)
    {
        await _context.MenuItemPictures.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task DeleteAsync(MenuItemPicture entity, CancellationToken cancellationToken = default)
    {
        _context.MenuItemPictures.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<MenuItemPicture>> GetByMenuItemIdAsync(Guid menuItemId, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItemPictures
            .Where(p => p.MenuItemId == menuItemId)
            .ToListAsync(cancellationToken);
    }

    public async Task<MenuItemPicture?> GetByIdAsync(Guid pictureId, CancellationToken cancellationToken = default)
    {
        return await _context.MenuItemPictures.FirstOrDefaultAsync(p => p.Id == pictureId, cancellationToken);
    }

    public Task UpdateAsync(MenuItemPicture entity, CancellationToken cancellationToken = default)
    {
        _context.MenuItemPictures.Update(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
