namespace Application.Abstractions.Services;

public interface IDeliverySimulationJob
{
    Task RunAsync(Guid trackingSessionId, CancellationToken cancellationToken = default);
}
