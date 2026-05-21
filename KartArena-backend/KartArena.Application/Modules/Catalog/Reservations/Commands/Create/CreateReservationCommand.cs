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

        public required int UserId { get; set; }
        public required int KartId { get; set; }
        public required int TrackId { get; set; }

        public required decimal Amount { get; set; }
        public required int PaymentTypeId { get; set; }
        public string? PaymentNote { get; set; }
    }
}