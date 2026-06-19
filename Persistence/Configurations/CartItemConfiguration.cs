using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(cartItem => cartItem.Id);

        builder.HasOne(cartItem => cartItem.MenuItem)
            .WithMany()
            .HasForeignKey(cartItem => cartItem.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
