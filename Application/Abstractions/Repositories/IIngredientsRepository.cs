using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IIngredientsRepository
{
    // CREATE
    Task<Ingredient> AddAsync(Ingredient ingredient, CancellationToken cancellationToken = default);

    // READ
    Task<Ingredient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ingredient>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ingredient>> GetPagedAsync(int skip, int take, string? searchTerm, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? searchTerm, CancellationToken cancellationToken = default);

    // UPDATE
    Task UpdateAsync(Ingredient ingredient, CancellationToken cancellationToken = default);

    // DELETE
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsUsedAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
