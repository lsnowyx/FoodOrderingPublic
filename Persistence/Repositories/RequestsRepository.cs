using Application.Abstractions.Repositories;
using Common.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;


namespace Persistence.Repositories;

public class RequestsRepository : IRequestsRepository
{
    private readonly AppDbContext _context;

    public RequestsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Request> AddAsync(Request request, CancellationToken cancellationToken = default)
    {
        await _context.Requests.AddAsync(request, cancellationToken);
        return request;
    }

    public async Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.Client)
            .Include(r => r.Worker)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.Client)
            .Include(r => r.Worker)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.Client)
            .Include(r => r.Worker)
            .Where(r => r.ClientId == clientId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetByWorkerIdAsync(Guid workerId, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.Client)
            .Include(r => r.Worker)
            .Where(r => r.WorkerId == workerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetWithNoWorkerAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.Client)
            .Include(r => r.Worker)
            .Where(x => x.WorkerId == null)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(Request request, CancellationToken cancellationToken = default)
    {
        _context.Requests.Update(request);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Requests.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _context.Requests.Remove(entity);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetWorkersWithoutRequestsAsync(CancellationToken cancellationToken = default)
    {
        var workerRoleId = await _context.Roles
            .Where(r => r.Name == UserRoleConstants.WORKER_ROLE)
            .Select(r => r.Id)
            .FirstAsync(cancellationToken);

        return await _context.Users
            .Where(u =>
                _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == workerRoleId)
                &&
                !_context.Requests.Any(r => r.WorkerId == u.Id && r.Completed == false))
            .ToListAsync(cancellationToken);
    }
}