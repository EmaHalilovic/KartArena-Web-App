using KartArena.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KartArena.Infrastructure.Persistence.Configurations
{
    public class PaymentConfiguration
        : IEntityTypeConfiguration<PaymentEntity>
    {
        public void Configure(
            EntityTypeBuilder<PaymentEntity> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.Currency)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.TransactionReference)
                .HasMaxLength(255);

            builder.Property(x => x.StripeCheckoutSessionId)
                .HasMaxLength(255);

            builder.Property(x => x.StripePaymentIntentId)
                .HasMaxLength(255);

            builder.Property(x => x.StripeEventId)
                .HasMaxLength(255);

            builder.Property(x => x.Note)
                .HasMaxLength(1000);

      

            builder.HasIndex(x => x.StripeCheckoutSessionId)
                .IsUnique();

            builder.HasIndex(x => x.StripePaymentIntentId);

          

            builder.HasOne(x => x.PaymentType)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.PaymentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}