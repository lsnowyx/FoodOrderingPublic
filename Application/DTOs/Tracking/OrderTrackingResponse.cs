using Application.DTOs.Order;

namespace Application.DTOs.Tracking;

public class OrderTrackingResponse
{
    public Guid OrderId { get; set; }

    public string Status { get; set; } = null!;

    public string DeliveryStatus { get; set; } = null!;

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public bool IsPaid { get; set; }

    public bool CanPayOnlineAgain { get; set; }

    public string PaymentMessage { get; set; } = null!;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public IReadOnlyList<CustomerOrderItemResponse> Items { get; set; } = Array.Empty<CustomerOrderItemResponse>();
}
