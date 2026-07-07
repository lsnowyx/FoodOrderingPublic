using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(order => order.Id);

        builder.Property(order => order.TotalAmount)
            .HasPrecision(18, 2);

        builder.HasOne(order => order.GuestCustomer)
            .WithMany()
            .HasForeignKey(order => order.GuestCustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(order => order.AssignedOrderManager)
            .WithMany()
            .HasForeignKey(order => order.AssignedOrderManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(order => order.OrderItems)
            .WithOne(orderItem => orderItem.Order)
            .HasForeignKey(orderItem => orderItem.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(order => order.PaymentAttempts)
            .WithOne(paymentAttempt => paymentAttempt.Order)
            .HasForeignKey(paymentAttempt => paymentAttempt.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
