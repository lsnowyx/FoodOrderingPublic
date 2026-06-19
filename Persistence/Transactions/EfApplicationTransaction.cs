using Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System.Data;

namespace Persistence.Transactions;

public sealed class EfApplicationTransaction : IApplicationTransaction
{
    private readonly AppDbContext _context;

    public EfApplicationTransaction(AppDbContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction is not null)
        {
            return await operation(cancellationToken);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var result = await operation(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return result;
    }
}
