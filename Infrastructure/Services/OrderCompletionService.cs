using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Common.Enums;

namespace Infrastructure.Services;

public class OrderCompletionService : IOrderCompletionService
{
    private readonly IOrderTrackingLinksRepository _trackingLinksRepository;

    public OrderCompletionService(IOrderTrackingLinksRepository trackingLinksRepository)
    {
        _trackingLinksRepository = trackingLinksRepository;
    }

    public async Task MarkDeliveredAsync(Domain.Entities.Order order, DateTime deliveredAt, CancellationToken cancellationToken = default)
    {
        if (!order.IsPaid)
        {
            throw new InvalidOperationException("Unpaid orders cannot be marked as delivered.");
        }

        order.Status = OrderStatus.Delivered;
        order.DeliveredAt ??= deliveredAt;
        order.UpdatedAt = deliveredAt;

        await _trackingLinksRepository.RevokeActiveByOrderIdAsync(order.Id, deliveredAt, cancellationToken);
    }
}
