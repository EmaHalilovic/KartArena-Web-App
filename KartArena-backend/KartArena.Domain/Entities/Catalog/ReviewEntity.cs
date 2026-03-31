using KartArena.Domain.Common;
using KartArena.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Catalog
{
    public class ReviewEntity: BaseEntity
    {
        public string? Comment { get; set; }
        public DateTime? Date { get; set; }

        // FK
        public int? KartArenaUsersId { get; set; }
        public UserEntity? Users { get; set; }
    }
}
