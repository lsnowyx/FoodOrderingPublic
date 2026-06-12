using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IMenuItemPicturesRepository
{
    Task<IEnumerable<MenuItemPicture>> GetByMenuItemIdAsync(Guid menuItemId, CancellationToken cancellationToken = default);
    Task<MenuItemPicture?> GetByIdAsync(Guid pictureId, CancellationToken cancellationToken = default);

    Task<MenuItemPicture> AddAsync(MenuItemPicture entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(MenuItemPicture entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(MenuItemPicture entity, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
