using KartArena.Domain.Entities.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Infrastructure.Database.Configurations.Catelog
{
    public class PaymentReservationConfiguration
     : IEntityTypeConfiguration<PaymentReservationEntity>
    {
        public void Configure(
            EntityTypeBuilder<PaymentReservationEntity> builder)
        {
            builder.ToTable("PaymentReservations");

            builder.HasKey(x => new
            {
                x.PaymentId,
                x.ReservationId
            });

            builder.HasOne(x => x.Payment)
                .WithMany(x => x.PaymentReservations)
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Reservation)
                .WithMany(x => x.PaymentReservations)
                .HasForeignKey(x => x.ReservationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ReservationId);
        }
    }
}
