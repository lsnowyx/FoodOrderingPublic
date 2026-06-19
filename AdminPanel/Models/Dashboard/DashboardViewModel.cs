using System.Collections.Generic;

namespace AdminPanel.Models.Dashboard;

public class DashboardViewModel
{
    public bool IsMenuManager { get; set; }
    public bool IsOrderManager { get; set; }
    public bool ShowOrderSummary { get; set; }
    public string? UserName { get; set; }

    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int PaidOrders { get; set; }
    public int UnpaidOrders { get; set; }
    public int PreparingOrders { get; set; }
    public int OutForDeliveryOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }

    public List<OrderSummaryViewModel>? RecentOrders { get; set; } = new();
}
