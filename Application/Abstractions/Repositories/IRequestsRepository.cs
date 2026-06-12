using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IRequestsRepository
{

    // CREATE
    Task<Request> AddAsync(Request request, CancellationToken cancellationToken = default);

    // READ
    Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetAllAsync(CancellationToken cancellationToken = default);

    // Optional filters
    Task<IEnumerable<Request>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Request>> GetByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default);

    // UPDATE
    Task UpdateAsync(Request request, CancellationToken cancellationToken = default);

    // DELETE
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // OPTIONAL: Save changes
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<User>> GetWorkersWithoutRequestsAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<Request>> GetWithNoWorkerAsync(CancellationToken cancellationToken = default);
}


