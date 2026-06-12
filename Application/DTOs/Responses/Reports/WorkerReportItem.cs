namespace Application.DTOs.Responses.Reports;

public sealed class WorkerReportItem
{
    public Guid WorkerId { get; set; }
    public string WorkerName { get; set; } = string.Empty;
    public int TotalAssignedRequests { get; set; }
    public int CompletedRequests { get; set; }
    public int ActiveRequests { get; set; }
}
