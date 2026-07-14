using KartArena.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Catalog
{
    public class KartEntity : BaseEntity
    {
        public string? Name { get; set; }
        public string? Colour { get; set; }
        public int? YearOfManufacture { get; set; }
        public string? ChassisNumber { get; set; }
        public string? Manufacturer { get; set; }
        public string? ImageUrl { get; set; }

        //removed '?' from PricePerSession because it should be required for reservation calculations
        public decimal PricePerSession { get; set; }
        public string? Description { get; set; }




        // FK
        public int? PowertrainTypeId { get; set; }
        public PowertrainTypeEntity? PowertrainType { get; set; }

        //Collections
        public IReadOnlyCollection<ServiceEntity>? Services { get; set; }


    }
}