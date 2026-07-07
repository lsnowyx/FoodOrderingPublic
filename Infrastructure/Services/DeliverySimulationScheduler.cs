using Application.Abstractions.Services;
using Hangfire;

namespace Infrastructure.Services;

public sealed class DeliverySimulationScheduler : IDeliverySimulationScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public DeliverySimulationScheduler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public string Enqueue(Guid trackingSessionId)
    {
        return _backgroundJobClient.Enqueue<IDeliverySimulationJob>(
            job => job.RunAsync(trackingSessionId, CancellationToken.None));
    }
}
