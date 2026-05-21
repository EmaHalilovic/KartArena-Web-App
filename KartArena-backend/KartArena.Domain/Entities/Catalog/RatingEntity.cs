using KartArena.Domain.Common;
using KartArena.Domain.Entities.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Catalog
{
    public class RatingEntity : BaseEntity
    {
        public int Score { get; set; }
        public string? Comment { get; set; }
        public DateTime? Date { get; set; }

        // FK
        public int? ReservationId { get; set; }
        public ReservationEntity? Reservation { get; set; }
    }
}
