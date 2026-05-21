using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;

namespace KartArena.Application.Modules.Catalog.Reservations.Queries.GetById
{
    public class GetReservationByIdQueryDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int TrackId { get; set; }
        public int KartId { get; set; }

        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public ReservationStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }

        public decimal? PaymentAmount { get; set; }
        public int? PaymentTypeId { get; set; }
        public string? PaymentTypeName { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? TransactionReference { get; set; }
        public string? PaymentNote { get; set; }

        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }

        public string? TrackName { get; set; }
        public string? KartName { get; set; }
    }
}