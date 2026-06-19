using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.HasKey(menuItem => menuItem.Id);

        builder.Property(menuItem => menuItem.Name)
            .IsRequired();

        builder.Property(menuItem => menuItem.Price)
            .HasPrecision(18, 2);

        builder.Property(menuItem => menuItem.RestaurantCost)
            .HasPrecision(18, 4);

        builder.Property(menuItem => menuItem.TotalCalories)
            .HasPrecision(18, 4);

        builder.HasMany(menuItem => menuItem.MenuItemIngredients)
            .WithOne(menuItemIngredient => menuItemIngredient.MenuItem)
            .HasForeignKey(menuItemIngredient => menuItemIngredient.MenuItemId);

        builder.HasMany(menuItem => menuItem.MenuItemPictures)
            .WithOne(picture => picture.MenuItem)
            .HasForeignKey(picture => picture.MenuItemId);
    }
}
