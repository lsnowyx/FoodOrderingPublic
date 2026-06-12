using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IOrdersRepository
{
    Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetAvailableAsync(CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetActiveAssignedToOrderManagerAsync(Guid orderManagerId, CancellationToken cancellationToken = default);

    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);

    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    // Additional helpers
    Task RecalculateTotalsAsync(Order order, CancellationToken cancellationToken = default);
}
