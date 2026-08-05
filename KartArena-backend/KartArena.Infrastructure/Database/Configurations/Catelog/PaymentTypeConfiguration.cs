using KartArena.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KartArena.Infrastructure.Persistence.Configurations
{
    public class PaymentTypeConfiguration
        : IEntityTypeConfiguration<PaymentTypeEntity>
    {
        public void Configure(
            EntityTypeBuilder<PaymentTypeEntity> builder)
        {
            builder.ToTable("PaymentTypes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.HasIndex(x => x.Code)
                .IsUnique();
        }
    }
}