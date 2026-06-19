using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class GuestCustomerConfiguration : IEntityTypeConfiguration<GuestCustomer>
{
    public void Configure(EntityTypeBuilder<GuestCustomer> builder)
    {
        builder.HasKey(guestCustomer => guestCustomer.Id);

        builder.Property(guestCustomer => guestCustomer.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(guestCustomer => guestCustomer.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(guestCustomer => guestCustomer.PhoneNumber)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(guestCustomer => guestCustomer.CreatedAt)
            .IsRequired();
    }
}
