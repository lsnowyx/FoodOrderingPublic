using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(orderItem => orderItem.Id);

        builder.HasOne(orderItem => orderItem.MenuItem)
            .WithMany()
            .HasForeignKey(orderItem => orderItem.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
