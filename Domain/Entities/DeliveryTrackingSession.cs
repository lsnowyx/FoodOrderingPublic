using Common.Enums;

namespace Domain.Entities;

public class DeliveryTrackingSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public decimal StartLatitude { get; set; }
    public decimal StartLongitude { get; set; }

    public decimal DestinationLatitude { get; set; }
    public decimal DestinationLongitude { get; set; }

    public decimal CurrentLatitude { get; set; }
    public decimal CurrentLongitude { get; set; }

    public decimal Progress { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ArrivedAt { get; set; }

    public int DurationSeconds { get; set; }
    public int UpdateIntervalSeconds { get; set; }

    public DeliveryTrackingStatus Status { get; set; } = DeliveryTrackingStatus.InProgress;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
