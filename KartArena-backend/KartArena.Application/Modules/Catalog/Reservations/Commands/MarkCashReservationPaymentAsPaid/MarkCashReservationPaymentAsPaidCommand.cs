using MediatR;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.MarkCashPaymentAsPaid
{
    public sealed class MarkCashReservationPaymentAsPaidCommand : IRequest<int>
    {
        public int ReservationId { get; set; }
        public string? TransactionReference { get; set; }
        public string? Note { get; set; }
    }
}