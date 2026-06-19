using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class OrderTrackingLinkConfiguration : IEntityTypeConfiguration<OrderTrackingLink>
{
    public void Configure(EntityTypeBuilder<OrderTrackingLink> builder)
    {
        builder.HasKey(trackingLink => trackingLink.Id);

        builder.Property(trackingLink => trackingLink.OrderId)
            .IsRequired();

        builder.Property(trackingLink => trackingLink.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(trackingLink => trackingLink.Scope)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(trackingLink => trackingLink.ExpiresAt)
            .IsRequired();

        builder.Property(trackingLink => trackingLink.CreatedAt)
            .IsRequired();

        builder.Property(trackingLink => trackingLink.RevokedAt)
            .IsRequired(false);

        builder.Property(trackingLink => trackingLink.LastUsedAt)
            .IsRequired(false);

        builder.HasIndex(trackingLink => trackingLink.TokenHash)
            .IsUnique();

        builder.HasOne(trackingLink => trackingLink.Order)
            .WithMany(order => order.TrackingLinks)
            .HasForeignKey(trackingLink => trackingLink.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
