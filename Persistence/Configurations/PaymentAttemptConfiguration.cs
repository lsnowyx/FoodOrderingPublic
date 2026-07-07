using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

public sealed class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.HasKey(paymentAttempt => paymentAttempt.Id);

        builder.Property(paymentAttempt => paymentAttempt.Amount)
            .HasPrecision(18, 2);

        builder.Property(paymentAttempt => paymentAttempt.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(paymentAttempt => paymentAttempt.StripeCheckoutSessionId)
            .HasMaxLength(255);

        builder.Property(paymentAttempt => paymentAttempt.StripeCheckoutUrl)
            .HasMaxLength(2048);

        builder.Property(paymentAttempt => paymentAttempt.StripePaymentIntentId)
            .HasMaxLength(255);

        builder.HasOne(paymentAttempt => paymentAttempt.Order)
            .WithMany(order => order.PaymentAttempts)
            .HasForeignKey(paymentAttempt => paymentAttempt.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
