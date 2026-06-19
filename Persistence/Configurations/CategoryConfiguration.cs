using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name)
            .IsRequired();

        builder.HasMany(category => category.MenuItems)
            .WithOne(menuItem => menuItem.Category)
            .HasForeignKey(menuItem => menuItem.CategoryId);
    }
}
