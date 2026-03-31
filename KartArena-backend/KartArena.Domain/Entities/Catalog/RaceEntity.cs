using KartArena.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Catalog
{
    public class RaceEntity : BaseEntity
    {
        public string? Name { get; set; }
        public DateTime? DateOfRace { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
      
        public int? LapCount { get; set; }

        // FK
        public int? TrackId { get; set; }
        public TrackEntity? Track { get; set; }

        public IReadOnlyCollection<RaceUserEntity>? KartArenaUsers { get; set; }
    }
}
