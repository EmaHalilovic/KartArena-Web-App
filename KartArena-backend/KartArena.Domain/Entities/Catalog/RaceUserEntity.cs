using KartArena.Domain.Common;
using KartArena.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Catalog
{
    public class RaceUserEntity : BaseEntity
    {

        public int StartPosition { get; set; }
        public string? FinalPosition { get; set; }
        public int Points { get; set; }

        //FK
        public int RaceId { get; set; }
        public RaceEntity? Race { get; set; }

        public int UserId { get; set; }
        public UserEntity? Users { get; set; }

        public int KartId { get; set; }
        public KartEntity? Kart { get; set; }

        //Collections
        public IReadOnlyCollection<LapTimeEntity>? LapsTime { get; set; }
    }
}
