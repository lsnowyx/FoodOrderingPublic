using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class MenuItemIngredientConfiguration : IEntityTypeConfiguration<MenuItemIngredient>
{
    public void Configure(EntityTypeBuilder<MenuItemIngredient> builder)
    {
        builder.HasKey(menuItemIngredient => menuItemIngredient.Id);

        builder.HasIndex(menuItemIngredient => new { menuItemIngredient.MenuItemId, menuItemIngredient.IngredientId })
            .IsUnique();

        builder.Property(menuItemIngredient => menuItemIngredient.Quantity)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.HasOne(menuItemIngredient => menuItemIngredient.MenuItem)
            .WithMany(menuItem => menuItem.MenuItemIngredients)
            .HasForeignKey(menuItemIngredient => menuItemIngredient.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(menuItemIngredient => menuItemIngredient.Ingredient)
            .WithMany(ingredient => ingredient.MenuItemIngredients)
            .HasForeignKey(menuItemIngredient => menuItemIngredient.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
