using KartArena.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KartArena.Infrastructure.Persistence.Configurations;

public sealed class PaymentAttemptConfiguration
    : IEntityTypeConfiguration<PaymentAttemptEntity>
{
    public void Configure(EntityTypeBuilder<PaymentAttemptEntity> builder)
    {
        builder.ToTable("PaymentAttempts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IdempotencyKey)
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(x => x.StripeCheckoutSessionId).HasMaxLength(255);
        builder.Property(x => x.CheckoutUrl).HasMaxLength(2048);
        builder.Property(x => x.StripeEventId).HasMaxLength(255);

        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        builder.HasIndex(x => x.StripeCheckoutSessionId)
            .IsUnique()
            .HasFilter("[StripeCheckoutSessionId] IS NOT NULL");
        builder.HasIndex(x => x.PaymentId)
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        builder.HasOne(x => x.Payment)
            .WithMany(x => x.PaymentAttempts)
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
