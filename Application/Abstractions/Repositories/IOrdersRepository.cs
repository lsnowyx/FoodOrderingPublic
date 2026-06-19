using Application.DTOs.Dashboard;
using Common.Enums;
using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IOrdersRepository
{
    Task<IReadOnlyList<Order>> GetPagedAsync(
        int skip,
        int take,
        OrderStatus? status,
        bool? isPaid,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(
        OrderStatus? status,
        bool? isPaid,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetAvailablePagedAsync(
        int skip,
        int take,
        OrderStatus? status,
        bool? isPaid,
        CancellationToken cancellationToken = default);
    Task<int> CountAvailableAsync(
        OrderStatus? status,
        bool? isPaid,
        CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Order?> GetActiveAssignedToOrderManagerAsync(Guid orderManagerId, CancellationToken cancellationToken = default);
    Task<bool> TryAssignAsync(
        Guid orderId,
        Guid orderManagerId,
        DateTime assignedAt,
        CancellationToken cancellationToken = default);
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync(
        int recentOrderCount,
        CancellationToken cancellationToken = default);

    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);

    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    // Additional helpers
    Task RecalculateTotalsAsync(Order order, CancellationToken cancellationToken = default);
}
