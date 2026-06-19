using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class MenuItemPictureConfiguration : IEntityTypeConfiguration<MenuItemPicture>
{
    public void Configure(EntityTypeBuilder<MenuItemPicture> builder)
    {
        builder.HasKey(picture => picture.Id);

        builder.Property(picture => picture.ImageUrl)
            .IsRequired();

        builder.Property(picture => picture.ImagePublicId)
            .IsRequired();

        builder.HasOne(picture => picture.MenuItem)
            .WithMany(menuItem => menuItem.MenuItemPictures)
            .HasForeignKey(picture => picture.MenuItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
