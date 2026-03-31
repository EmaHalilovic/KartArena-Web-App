using KartArena.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Catalog
{
    public class PowertrainTypeEntity : BaseEntity
    {
        public string? Name { get; set; }
        public string? Manufacturer { get; set; }
        public string? PowerHP { get; set; }
        public string? EngineCapacity { get; set; }

        //Collections

        public IReadOnlyCollection<KartEntity>? Karts { get; set; }

    }
}
