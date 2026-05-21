using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.MarkCashPaymentAsPaid
{
    public sealed class MarkCashReservationPaymentAsPaidCommandHandler(IAppDbContext ctx)
        : IRequestHandler<MarkCashReservationPaymentAsPaidCommand, int>
    {
        public async Task<int> Handle(MarkCashReservationPaymentAsPaidCommand request, CancellationToken ct)
        {
            var reservation = await ctx.Reservations
                .Include(r => r.Payment)
                    .ThenInclude(p => p.PaymentType)
                .FirstOrDefaultAsync(r => r.Id == request.ReservationId && !r.IsDeleted, ct);

            if (reservation is null)
                throw new Exception("Reservation not found.");

            if (reservation.Payment is null)
                throw new Exception("Payment does not exist for this reservation.");

            if (reservation.Payment.Status != PaymentStatus.Pending)
                throw new Exception("Only pending payments can be confirmed.");

            if (reservation.Payment.PaymentType is null)
                throw new Exception("Payment type is missing.");

            if (!reservation.Payment.PaymentType.isEnabled)
                throw new Exception("Payment type is not active.");

            if (!reservation.Payment.PaymentType.AllowedAtDesk)
                throw new Exception("Payment type is not available at arena.");

            //if (reservation.Payment.PaymentType.Code != "CASH")
            //    throw new Exception("Only cash payments can be confirmed manually.");

            reservation.Payment.Status = PaymentStatus.Paid;
            reservation.Payment.PaymentDate = DateTime.UtcNow;
            reservation.Payment.TransactionReference = request.TransactionReference;
            reservation.Payment.Note = request.Note;

            reservation.PaymentStatus = PaymentStatus.Paid;

            if (reservation.Status == ReservationStatus.Pending)
            {
                reservation.Status = ReservationStatus.Confirmed;
            }

            await ctx.SaveChangesAsync(ct);

            return reservation.Id;
        }
    }
}