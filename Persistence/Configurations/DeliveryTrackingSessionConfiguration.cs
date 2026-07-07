using Common.Enums;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class DeliveryTrackingSessionConfiguration : IEntityTypeConfiguration<DeliveryTrackingSession>
{
    public void Configure(EntityTypeBuilder<DeliveryTrackingSession> builder)
    {
        builder.HasKey(session => session.Id);

        builder.Property(session => session.OrderId)
            .IsRequired();

        builder.Property(session => session.StartLatitude)
            .HasPrecision(18, 12)
            .IsRequired();

        builder.Property(session => session.StartLongitude)
            .HasPrecision(18, 12)
            .IsRequired();

        builder.Property(session => session.DestinationLatitude)
            .HasPrecision(18, 12)
            .IsRequired();

        builder.Property(session => session.DestinationLongitude)
            .HasPrecision(18, 12)
            .IsRequired();

        builder.Property(session => session.CurrentLatitude)
            .HasPrecision(18, 12)
            .IsRequired();

        builder.Property(session => session.CurrentLongitude)
            .HasPrecision(18, 12)
            .IsRequired();

        builder.Property(session => session.Progress)
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(session => session.StartedAt)
            .IsRequired();

        builder.Property(session => session.ArrivedAt)
            .IsRequired(false);

        builder.Property(session => session.DurationSeconds)
            .IsRequired();

        builder.Property(session => session.UpdateIntervalSeconds)
            .IsRequired();

        builder.Property(session => session.Status)
            .IsRequired();

        builder.Property(session => session.CreatedAt)
            .IsRequired();

        builder.Property(session => session.UpdatedAt)
            .IsRequired(false);

        builder.HasIndex(session => session.OrderId)
            .HasDatabaseName("IX_DeliveryTrackingSessions_OrderId");

        builder.HasIndex(
                session => session.OrderId,
                "IX_DeliveryTrackingSessions_OrderId_InProgress")
            .IsUnique()
            .HasFilter($"[{nameof(DeliveryTrackingSession.Status)}] = {(int)DeliveryTrackingStatus.InProgress}");

        builder.HasOne(session => session.Order)
            .WithMany(order => order.DeliveryTrackingSessions)
            .HasForeignKey(session => session.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
