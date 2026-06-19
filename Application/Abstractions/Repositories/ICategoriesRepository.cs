using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface ICategoriesRepository
{
    // CREATE
    Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);

    // READ
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetPagedAsync(int skip, int take, string? searchTerm, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? searchTerm, CancellationToken cancellationToken = default);

    // UPDATE
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);

    // DELETE
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
