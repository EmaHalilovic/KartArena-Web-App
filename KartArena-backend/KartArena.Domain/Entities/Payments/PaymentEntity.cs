using KartArena.Domain.Common;
using KartArena.Domain.Entities.Reservations;

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

        // FK Reservation
        public int ReservationId { get; set; }

        public ReservationEntity Reservation { get; set; } = null!;
    }
}