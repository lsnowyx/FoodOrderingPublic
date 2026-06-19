namespace AdminPanel.Models.Order;

public class OrderViewModel
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public Guid? GuestCustomerId { get; set; }
    public Guid? CustomerId => UserId ?? GuestCustomerId;
    public Guid? AssignedOrderManagerId { get; set; }
    public DateTime? AssignedAt { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public string? DeliveryAddress { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IEnumerable<OrderItemViewModel> Items { get; set; } = Enumerable.Empty<OrderItemViewModel>();

    public decimal Total => Items?.Sum(i => i.LineTotal) ?? 0m;
}
