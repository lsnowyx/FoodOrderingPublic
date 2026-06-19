using AdminPanel.Models.Common;
using Common.Enums;

namespace AdminPanel.Models.Order;

public sealed class OrderIndexViewModel
{
    public OrderStatus? Status { get; set; }
    public bool? IsPaid { get; set; }
    public PaginatedViewModel<OrderViewModel> Orders { get; set; } = new();
}
