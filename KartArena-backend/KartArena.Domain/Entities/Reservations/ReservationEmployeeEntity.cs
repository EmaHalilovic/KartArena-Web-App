using KartArena.Domain.Common;
using KartArena.Domain.Entities.Equipment;
using KartArena.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Reservations
{
    public class ReservationEmployeeEntity : BaseEntity
    {
        //fk
        public int EmployeeId { get; set; }
        public UserEntity? Employee { get; set; } = default;

        public int ReservationId { get; set; }
        public ReservationEntity? Reservation { get; set; } = default;

        // optional because not every employee assignment must include equipment
        public int? EquipmentItemId { get; set; }
        public EquipmentItemEntity? EquipmentItem { get; set; }

    }
}
