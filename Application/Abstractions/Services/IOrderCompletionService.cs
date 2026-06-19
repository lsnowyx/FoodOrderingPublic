using Domain.Entities;

namespace Application.Abstractions.Services;

public interface IOrderCompletionService
{
    Task MarkDeliveredAsync(Order order, DateTime deliveredAt, CancellationToken cancellationToken = default);
}
