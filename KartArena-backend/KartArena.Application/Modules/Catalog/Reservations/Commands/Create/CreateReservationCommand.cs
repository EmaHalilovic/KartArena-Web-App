using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using MediatR;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Create
{
    public sealed class CreateReservationCommand : IRequest<int>
    {
        public required DateTime ReservationDate { get; set; }
        public required DateTime StartTime { get; set; }
        public required DateTime EndTime { get; set; }

        public int? UserId { get; set; }
        public string CustomerFirstName { get; set; } = string.Empty;
        public string CustomerLastName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public required int KartId { get; set; }
        public required int TrackId { get; set; }

        public decimal? Amount { get; set; }
        public int? PaymentTypeId { get; set; }
        public string? PaymentNote { get; set; }
    }
}
