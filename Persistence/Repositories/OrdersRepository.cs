using Application.Abstractions.Repositories;
using Common.Enums;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class OrdersRepository : IOrdersRepository
{
    private readonly AppDbContext _context;

    public OrdersRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Customer)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Customer)
            .Where(o => o.AssignedOrderManagerId == null
                && o.Status != OrderStatus.Delivered
                && o.Status != OrderStatus.Cancelled)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetActiveAssignedToOrderManagerAsync(Guid orderManagerId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.AssignedOrderManagerId == orderManagerId
                && o.Status != OrderStatus.Delivered
                && o.Status != OrderStatus.Cancelled,
                cancellationToken);
    }

    public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        return Task.CompletedTask;
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        return order;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task RecalculateTotalsAsync(Order order, CancellationToken cancellationToken = default)
    {
        // For now totals are implicit: sum of OrderItems (Quantity * UnitPrice)
        // If you store a Total on Order, set it here. Keeping method for extensibility.
        return Task.CompletedTask;
    }
}
