using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasMany(user => user.ClientRequest)
            .WithOne(request => request.Client)
            .HasForeignKey(request => request.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(user => user.WorkerRequest)
            .WithOne(request => request.Worker)
            .HasForeignKey(request => request.WorkerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(user => user.Orders)
            .WithOne(order => order.User)
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(user => user.Cart)
            .WithOne(cart => cart.Customer)
            .HasForeignKey<Cart>(cart => cart.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.AuditLogs)
            .WithOne(auditLog => auditLog.User)
            .HasForeignKey(auditLog => auditLog.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
