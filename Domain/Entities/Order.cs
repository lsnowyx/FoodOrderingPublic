using Common.Enums;
namespace Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public Guid? GuestCustomerId { get; set; }
    public GuestCustomer? GuestCustomer { get; set; }

    public Guid? AssignedOrderManagerId { get; set; }
    public User? AssignedOrderManager { get; set; }
    public DateTime? AssignedAt { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public bool IsPaid { get; set; }

    public decimal TotalAmount { get; set; }

    public string? DeliveryAddress { get; set; }

    public double? DeliveryLatitude { get; set; }
    public double? DeliveryLongitude { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? UpdatedAt { get; set; }


    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    public DateTime? PaidAt { get; set; }

    public List<PaymentAttempt> PaymentAttempts { get; set; } = new List<PaymentAttempt>();
    public List<OrderItem> OrderItems { get; set; } = new();
    public List<OrderTrackingLink> TrackingLinks { get; set; } = new();
    public List<DeliveryTrackingSession> DeliveryTrackingSessions { get; set; } = new();
}
