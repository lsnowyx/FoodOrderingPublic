using Domain.Entities;

namespace Application.Abstractions.Services;

public interface IDeliveryTrackingAccessService
{
    Task<Order> GetGuestTrackedOrderAsync(
        string trackingToken,
        CancellationToken cancellationToken = default);

    Task<Order> GetGuestTrackedOrderAsync(
        Guid orderId,
        string trackingToken,
        CancellationToken cancellationToken = default);

    Task<Order?> GetCustomerTrackedOrderAsync(
        Guid orderId,
        Guid customerId,
        CancellationToken cancellationToken = default);
}
