using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KartArena.Domain.Entities.Catalog;

namespace KartArena.Infrastructure.Database.Configurations.Catelog
{


    public class CitiesConfiguration : IEntityTypeConfiguration<CityEntity>
    {
        public void Configure(EntityTypeBuilder<CityEntity> builder)
        {
            builder
                .ToTable("Cities");

            

        }
    }


}
