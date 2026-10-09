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

        public string Currency { get; set; } = "bam";

        public DateTime? PaymentDate { get; set; }

        public PaymentStatus Status { get; set; }
            = PaymentStatus.Pending;


        public string? TransactionReference { get; set; }


        public string? StripeCheckoutSessionId { get; set; }


        public string? StripePaymentIntentId { get; set; }


        public string? StripeEventId { get; set; }

        public string? Note { get; set; }

        // FK PaymentType
        public int? PaymentTypeId { get; set; }

        public PaymentTypeEntity? PaymentType { get; set; }

        //  Reservation connecting table
        public ICollection<PaymentReservationEntity> PaymentReservations { get; set; } = new List<PaymentReservationEntity>();

        public ICollection<PaymentAttemptEntity> PaymentAttempts { get; set; } = new List<PaymentAttemptEntity>();


    }
}
