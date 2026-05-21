using KartArena.Domain.Common;
using KartArena.Domain.Entities.Catalog;
using KartArena.Domain.Entities.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Payments
{
    public class PaymentEntity : BaseEntity
    {
        public decimal Amount { get; set; }
        public DateTime? PaymentDate { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? TransactionReference { get; set; }
        public string? Note { get; set; }
        // FK
        public int? PaymentTypeId { get; set; }
        public PaymentTypeEntity? PaymentType { get; set; }

        // FK Reservation 
        public int ReservationId { get; set; }
        public ReservationEntity Reservation { get; set; } = null!;


    }
}
