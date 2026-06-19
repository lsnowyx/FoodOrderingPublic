namespace Application.DTOs.Dashboard;

public sealed record DashboardOrderSummaryResponse(
    Guid Id,
    DateTime OrderDate,
    string Status,
    bool IsPaid);
