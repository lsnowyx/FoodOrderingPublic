namespace WebClient.Models;

public sealed class AdminReportResponse
{
    public int TotalRequests { get; set; }
    public int CompletedRequests { get; set; }
    public int PendingRequests { get; set; }
    public int AssignedRequests { get; set; }
    public int UnassignedRequests { get; set; }
    public double CompletionPercentage { get; set; }

    public List<WorkerReportItem> Workers { get; set; } = new();
}
