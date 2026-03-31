using KartArena.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Catalog
{
    public class LapTimeEntity : BaseEntity
    {
        public TimeSpan? LapTime { get; set; }
        public int CurrentPosition { get; set; }

        //FK
        public int? RaceUserId { get; set; }
        public RaceUserEntity? RacesUsers { get; set; }

    }
}
