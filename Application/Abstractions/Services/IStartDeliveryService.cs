using OrderEntity = Domain.Entities.Order;

namespace Application.Abstractions.Services;

public interface IStartDeliveryService
{
    Task<OrderEntity> StartAsync(
        Guid orderId,
        Guid orderManagerId,
        CancellationToken cancellationToken = default);
}
