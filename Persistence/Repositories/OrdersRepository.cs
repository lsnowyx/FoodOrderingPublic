using Application.Abstractions.Repositories;
using Application.DTOs.Dashboard;
using Common.Enums;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System.Data;

namespace Persistence.Repositories;

public class OrdersRepository : IOrdersRepository
{
    private readonly AppDbContext _context;

    public OrdersRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Order>> GetPagedAsync(
        int skip,
        int take,
        OrderStatus? status,
        bool? isPaid,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(GetOrdersQuery(), status, isPaid)
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        OrderStatus? status,
        bool? isPaid,
        CancellationToken cancellationToken = default)
    {
        return ApplyFilters(_context.Orders.AsNoTracking(), status, isPaid)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetAvailablePagedAsync(
        int skip,
        int take,
        OrderStatus? status,
        bool? isPaid,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(GetAvailableOrdersQuery(), status, isPaid)
            .OrderBy(order => order.OrderDate)
            .ThenBy(order => order.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAvailableAsync(
        OrderStatus? status,
        bool? isPaid,
        CancellationToken cancellationToken = default)
    {
        return ApplyFilters(
                _context.Orders
                    .AsNoTracking()
                    .Where(IsAvailable()),
                status,
                isPaid)
            .CountAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.User)
            .Include(o => o.GuestCustomer)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order?> GetActiveAssignedToOrderManagerAsync(Guid orderManagerId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Include(o => o.User)
            .Include(o => o.GuestCustomer)
            .FirstOrDefaultAsync(o => o.AssignedOrderManagerId == orderManagerId
                && o.Status != OrderStatus.Delivered
                && o.Status != OrderStatus.Cancelled,
                cancellationToken);
    }

    public async Task<bool> TryAssignAsync(
        Guid orderId,
        Guid orderManagerId,
        DateTime assignedAt,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var managerHasActiveOrder = await _context.Orders.AnyAsync(
            order => order.AssignedOrderManagerId == orderManagerId
                && order.Status != OrderStatus.Delivered
                && order.Status != OrderStatus.Cancelled,
            cancellationToken);

        if (managerHasActiveOrder)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        var affectedRows = await _context.Orders
            .Where(order => order.Id == orderId
                && order.AssignedOrderManagerId == null
                && order.Status != OrderStatus.Delivered
                && order.Status != OrderStatus.Cancelled)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(order => order.AssignedOrderManagerId, (Guid?)orderManagerId)
                    .SetProperty(order => order.AssignedAt, assignedAt)
                    .SetProperty(order => order.UpdatedAt, assignedAt),
                cancellationToken);

        if (affectedRows != 1)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(
        int recentOrderCount,
        CancellationToken cancellationToken = default)
    {
        var groupedCounts = await _context.Orders
            .AsNoTracking()
            .GroupBy(order => new { order.Status, order.IsPaid })
            .Select(group => new
            {
                group.Key.Status,
                group.Key.IsPaid,
                Count = group.Count()
            })
            .ToListAsync(cancellationToken);

        var recentOrders = await _context.Orders
            .AsNoTracking()
            .OrderByDescending(order => order.OrderDate)
            .ThenByDescending(order => order.Id)
            .Take(Math.Max(recentOrderCount, 0))
            .Select(order => new DashboardOrderSummaryResponse(
                order.Id,
                order.OrderDate,
                order.Status.ToString(),
                order.IsPaid))
            .ToListAsync(cancellationToken);

        return new DashboardSummaryResponse(
            groupedCounts.Sum(group => group.Count),
            groupedCounts
                .Where(group => group.Status == OrderStatus.Pending)
                .Sum(group => group.Count),
            groupedCounts
                .Where(group => group.IsPaid)
                .Sum(group => group.Count),
            groupedCounts
                .Where(group => !group.IsPaid)
                .Sum(group => group.Count),
            groupedCounts
                .Where(group => group.Status == OrderStatus.Preparing)
                .Sum(group => group.Count),
            groupedCounts
                .Where(group => group.Status == OrderStatus.OutForDelivery)
                .Sum(group => group.Count),
            groupedCounts
                .Where(group => group.Status == OrderStatus.Delivered)
                .Sum(group => group.Count),
            groupedCounts
                .Where(group => group.Status == OrderStatus.Cancelled)
                .Sum(group => group.Count),
            recentOrders);
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

    private IQueryable<Order> GetOrdersQuery()
    {
        return _context.Orders
            .AsNoTracking()
            .Include(order => order.OrderItems)
                .ThenInclude(orderItem => orderItem.MenuItem)
            .Include(order => order.User)
            .Include(order => order.GuestCustomer);
    }

    private IQueryable<Order> GetAvailableOrdersQuery()
    {
        return GetOrdersQuery().Where(IsAvailable());
    }

    private static System.Linq.Expressions.Expression<Func<Order, bool>> IsAvailable()
    {
        return order => order.AssignedOrderManagerId == null
            && order.Status != OrderStatus.Delivered
            && order.Status != OrderStatus.Cancelled;
    }

    private static IQueryable<Order> ApplyFilters(
        IQueryable<Order> query,
        OrderStatus? status,
        bool? isPaid)
    {
        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        if (isPaid.HasValue)
        {
            query = query.Where(order => order.IsPaid == isPaid.Value);
        }

        return query;
    }
}
