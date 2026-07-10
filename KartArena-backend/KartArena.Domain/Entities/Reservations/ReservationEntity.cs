using KartArena.Domain.Common;
using KartArena.Domain.Entities.Catalog;
using KartArena.Domain.Entities.Identity;
using KartArena.Domain.Entities.Payments;

namespace KartArena.Domain.Entities.Reservations
{
    public class ReservationEntity : BaseEntity
    {
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        // optional but very useful
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        // FK
        public int TrackId { get; set; }
        public TrackEntity? Track { get; set; }

        public int KartId { get; set; }
        public KartEntity? Kart { get; set; }

        // Logged-in user, optional
        public int? UserId { get; set; }
        public UserEntity? User { get; set; }

        // Public reservation / guest info
        public string CustomerFirstName { get; set; } = string.Empty;
        public string CustomerLastName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        // one-to-one payment
        public PaymentEntity? Payment { get; set; }

        public ICollection<ReservationEmployeeEntity> Employees { get; set; } = new List<ReservationEmployeeEntity>();
    }
}
