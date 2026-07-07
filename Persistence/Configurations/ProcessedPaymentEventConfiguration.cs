using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class ProcessedPaymentEventConfiguration : IEntityTypeConfiguration<ProcessedPaymentEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedPaymentEvent> builder)
    {
        builder.HasKey(processedPaymentEvent => processedPaymentEvent.Id);

        builder.Property(processedPaymentEvent => processedPaymentEvent.Provider)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(processedPaymentEvent => processedPaymentEvent.ProviderEventId)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(processedPaymentEvent => processedPaymentEvent.EventType)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(processedPaymentEvent => processedPaymentEvent.RawStatus)
            .HasMaxLength(100);

        builder.HasIndex(processedPaymentEvent => new
            {
                processedPaymentEvent.Provider,
                processedPaymentEvent.ProviderEventId
            })
            .IsUnique();

        builder.HasOne(processedPaymentEvent => processedPaymentEvent.PaymentAttempt)
            .WithMany()
            .HasForeignKey(processedPaymentEvent => processedPaymentEvent.PaymentAttemptId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(processedPaymentEvent => processedPaymentEvent.Order)
            .WithMany()
            .HasForeignKey(processedPaymentEvent => processedPaymentEvent.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
