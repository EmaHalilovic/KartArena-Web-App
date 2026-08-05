using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.MarkCashPaymentAsPaid;

public sealed class MarkCashReservationPaymentAsPaidCommandHandler(
    IAppDbContext ctx)
    : IRequestHandler<MarkCashReservationPaymentAsPaidCommand, int>
{
    public async Task<int> Handle(
        MarkCashReservationPaymentAsPaidCommand request,
        CancellationToken ct)
    {
        var reservationExists = await ctx.Reservations.AnyAsync(
            reservation =>
                reservation.Id == request.ReservationId &&
                !reservation.IsDeleted,
            ct);

        if (!reservationExists)
        {
            throw new InvalidOperationException(
                "Reservation not found.");
        }

        var payment = await ctx.Payments
            .Include(p => p.PaymentType)
            .Include(p => p.PaymentReservations)
                .ThenInclude(link => link.Reservation)
            .FirstOrDefaultAsync(
                p =>
                    !p.IsDeleted &&
                    p.PaymentReservations.Any(link =>
                        link.ReservationId ==
                        request.ReservationId),
                ct);

        if (payment is null)
        {
            throw new InvalidOperationException(
                "Payment does not exist for this reservation.");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending payments can be confirmed.");
        }

        if (payment.PaymentType is null)
        {
            throw new InvalidOperationException(
                "Payment type is missing.");
        }

        if (!payment.PaymentType.isEnabled)
        {
            throw new InvalidOperationException(
                "Payment type is not active.");
        }

        if (!payment.PaymentType.AllowedAtDesk)
        {
            throw new InvalidOperationException(
                "Payment type is not available at arena.");
        }

        if (!string.Equals(
                payment.PaymentType.Code,
                "CASH",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only cash payments can be confirmed manually.");
        }

        var now = DateTime.UtcNow;

        payment.Status = PaymentStatus.Paid;
        payment.PaymentDate = now;
        payment.TransactionReference =
            request.TransactionReference?.Trim();
        payment.Note = request.Note?.Trim();
        payment.ModifiedAtUtc = now;

        /*
         * Confirm all reservations linked to this payment.
         */
        foreach (var link in payment.PaymentReservations)
        {
            var reservation = link.Reservation;

            reservation.PaymentStatus =
                PaymentStatus.Paid;

            if (reservation.Status ==
                ReservationStatus.Pending)
            {
                reservation.Status =
                    ReservationStatus.Confirmed;
            }

            reservation.ModifiedAtUtc = now;
        }

        await ctx.SaveChangesAsync(ct);

        return request.ReservationId;
    }
}