using KartArena.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Catalog
{
    public class RaceResultEntity : BaseEntity
    {
        public int LapCount { get; set; }
        public TimeSpan? TotalTime { get; set; }


        // FK
        public int RaceUserId { get; set; }
        public RaceUserEntity? RaceUsers { get; set; }


    }
}
