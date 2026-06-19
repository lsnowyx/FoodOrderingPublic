namespace Application.DTOs.Tracking;

public class OrderTrackingResponse
{
    public Guid OrderId { get; set; }

    public string Status { get; set; } = null!;

    public string DeliveryStatus { get; set; } = null!;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }
}
