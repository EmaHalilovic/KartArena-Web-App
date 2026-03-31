using KartArena.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Catalog
{
    public class TrackEntity : BaseEntity
    {
        public string? Name { get; set; }
        public double? Length { get; set; }
        public string? SurfaceType { get; set; }
        public bool? Outdoors { get; set; }

        // FK
        public int? CityId { get; set; }
        public CityEntity? City { get; set; }

        public IReadOnlyCollection<RaceEntity>? Races { get; set; }
    }
}
