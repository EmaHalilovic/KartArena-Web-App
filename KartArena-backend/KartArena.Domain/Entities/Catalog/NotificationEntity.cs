using KartArena.Domain.Common;
using KartArena.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Catalog
{
    public class NotificationEntity:BaseEntity
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime? SentAt { get; set; }
        public bool Status { get; set; }

        public int? UserId { get; set; }
        public UserEntity? Users { get; set; }
    }
}
