namespace Application.Abstractions.Services;

public interface IDeliverySimulationScheduler
{
    string Enqueue(Guid trackingSessionId);
}
