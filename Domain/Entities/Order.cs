using Common.Enums;
namespace Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public Guid? AssignedOrderManagerId { get; set; }
    public User? AssignedOrderManager { get; set; }
    public DateTime? AssignedAt { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public bool IsPaid { get; set; }

    public string? DeliveryAddress { get; set; }

    public double? DeliveryLatitude { get; set; }
    public double? DeliveryLongitude { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new();
}
