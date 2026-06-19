using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.HasKey(ingredient => ingredient.Id);

        builder.Property(ingredient => ingredient.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ingredient => ingredient.BaseUnit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ingredient => ingredient.UnitCost)
            .HasPrecision(18, 4);

        builder.Property(ingredient => ingredient.AllergenInfo)
            .HasMaxLength(500);

        builder.HasMany(ingredient => ingredient.MenuItemIngredients)
            .WithOne(menuItemIngredient => menuItemIngredient.Ingredient)
            .HasForeignKey(menuItemIngredient => menuItemIngredient.IngredientId);
    }
}
