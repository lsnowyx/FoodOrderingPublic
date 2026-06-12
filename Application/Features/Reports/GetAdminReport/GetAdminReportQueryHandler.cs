using Application.Abstractions.Repositories;
using Application.DTOs.Responses.Reports;
using MediatR;

namespace Application.Features.Reports.GetAdminReport;

public sealed class GetAdminReportQueryHandler : IRequestHandler<GetAdminReportQuery, AdminReportResponse>
{
    private readonly IRequestsRepository requestsRepository;

    public GetAdminReportQueryHandler(IRequestsRepository requestsRepository)
    {
        this.requestsRepository = requestsRepository;
    }

    public async Task<AdminReportResponse> Handle(
        GetAdminReportQuery request,
        CancellationToken cancellationToken)
    {
        var requests = (await requestsRepository.GetAllAsync(cancellationToken)).ToList();

        var totalRequests = requests.Count;
        var completedRequests = requests.Count(x => x.Completed);
        var pendingRequests = requests.Count(x => !x.Completed);
        var assignedRequests = requests.Count(x => x.WorkerId != null);
        var unassignedRequests = requests.Count(x => x.WorkerId == null);

        var workers = requests
            .Where(x => x.WorkerId != null)
            .GroupBy(x => x.WorkerId!.Value)
            .Select(group =>
            {
                var firstRequest = group.First();

                return new WorkerReportItem
                {
                    WorkerId = group.Key,
                    WorkerName = firstRequest.Worker?.UserName ?? "Unknown",
                    TotalAssignedRequests = group.Count(),
                    CompletedRequests = group.Count(x => x.Completed),
                    ActiveRequests = group.Count(x => !x.Completed)
                };
            })
            .ToList();

        return new AdminReportResponse
        {
            TotalRequests = totalRequests,
            CompletedRequests = completedRequests,
            PendingRequests = pendingRequests,
            AssignedRequests = assignedRequests,
            UnassignedRequests = unassignedRequests,
            CompletionPercentage = totalRequests == 0
                ? 0
                : Math.Round(completedRequests * 100.0 / totalRequests, 2),
            Workers = workers
        };
    }
}