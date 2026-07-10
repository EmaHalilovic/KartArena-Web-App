using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KartArena.Domain.Entities.Catalog;
using KartArena.Domain.Entities.Reservations;

namespace KartArena.Infrastructure.Database.Configurations.Catelog
{


    public class ReservationConfiguration : IEntityTypeConfiguration<ReservationEntity>
    {
        public void Configure(EntityTypeBuilder<ReservationEntity> builder)
        {
            builder.Property(x => x.TotalPrice)
     .HasPrecision(18, 2)
     .IsRequired();



        }
    }


}
