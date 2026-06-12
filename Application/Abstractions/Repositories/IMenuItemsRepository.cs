using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IMenuItemsRepository
{
    Task<MenuItem> AddAsync(MenuItem menuItem, CancellationToken cancellationToken = default);

    Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<MenuItem>> GetAllAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(MenuItem menuItem, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
