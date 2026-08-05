using KartArena.Domain.Entities.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Payments
{
    public class PaymentReservationEntity
    {
        public int PaymentId { get; set; }

        public PaymentEntity Payment { get; set; } = null!;

        public int ReservationId { get; set; }

        public ReservationEntity Reservation { get; set; } = null!;
    }
}
