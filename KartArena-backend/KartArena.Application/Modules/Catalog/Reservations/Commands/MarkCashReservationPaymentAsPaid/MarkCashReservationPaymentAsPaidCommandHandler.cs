using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.MarkCashPaymentAsPaid;

public sealed class MarkCashReservationPaymentAsPaidCommandHandler(
    IAppDbContext ctx, IBusinessClock businessClock)
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
            throw new MarketNotFoundException(
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
            throw new MarketNotFoundException(
                "Payment does not exist for this reservation.");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            throw new MarketBusinessRuleException(
             "PAYMENT_NOT_PENDING",
             "Only pending payments can be confirmed.");
        }

        if (payment.PaymentType is null)
        {
            throw new MarketBusinessRuleException(
     "PAYMENT_TYPE_MISSING",
     "Payment type is missing.");
        }

        if (!payment.PaymentType.isEnabled)
        {
            throw new MarketBusinessRuleException(
    "PAYMENT_TYPE_DISABLED",
    "Payment type is not active.");
        }

        if (!payment.PaymentType.AllowedAtDesk)
        {
            throw new MarketBusinessRuleException(
     "PAYMENT_TYPE_NOT_ALLOWED_AT_DESK",
     "Payment type is not available at the arena.");
        }

        if (!string.Equals(
                payment.PaymentType.Code,
                "DESK_CASH",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new MarketBusinessRuleException(
    "PAYMENT_NOT_CASH",
    "Only cash payments can be confirmed manually.");
        }

       

        payment.Status = PaymentStatus.Paid;
        payment.PaymentDate = businessClock.UtcNow;
        payment.TransactionReference =
            request.TransactionReference?.Trim();
        payment.Note = request.Note?.Trim();

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

        }

        await ctx.SaveChangesAsync(ct);

        return request.ReservationId;
    }
}