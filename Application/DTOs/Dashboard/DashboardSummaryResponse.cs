namespace Application.DTOs.Dashboard;

public sealed record DashboardSummaryResponse(
    int TotalOrders,
    int PendingOrders,
    int PaidOrders,
    int UnpaidOrders,
    int PreparingOrders,
    int OutForDeliveryOrders,
    int DeliveredOrders,
    int CancelledOrders,
    IReadOnlyList<DashboardOrderSummaryResponse> RecentOrders);
