using KartArena.Domain.Common;
using KartArena.Domain.Entities.Identity;
using KartArena.Domain.Entities.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Catalog
{
    public class ReservationEntity:BaseEntity
    {
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
      

        // FK
        public int TrackId { get; set; }
        public TrackEntity? Track { get; set; }

        public int KartId { get; set; }
        public KartEntity? Kart { get; set; }

      
        public PaymentEntity? Payment { get; set; }

        public int UserId { get; set; }
        public UserEntity? User { get; set; }

        public IReadOnlyCollection<ReservationEmployeeEntity> Employees { get; set; }

      
    }
}
