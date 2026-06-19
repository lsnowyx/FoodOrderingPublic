using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(auditLog => auditLog.Id);

        builder.Property(auditLog => auditLog.HttpMethod)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(auditLog => auditLog.Endpoint)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(auditLog => auditLog.RequestBody)
            .HasMaxLength(10000);

        builder.Property(auditLog => auditLog.ResponseBody)
            .HasMaxLength(10000);

        builder.Property(auditLog => auditLog.Timestamp)
            .IsRequired();

        builder.HasOne(auditLog => auditLog.User)
            .WithMany(user => user.AuditLogs)
            .HasForeignKey(auditLog => auditLog.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
