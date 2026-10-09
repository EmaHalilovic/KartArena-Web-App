using KartArena.Application.Modules.Catalog.Reservations.Services;
using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Update;

public sealed class UpdateReservationCommandHandler(
    IAppDbContext ctx,
    IBusinessClock businessClock,
    IReservationRulesService reservationRulesService)
    : IRequestHandler<UpdateReservationCommand, int>
{
    public async Task<int> Handle(UpdateReservationCommand request, CancellationToken ct)
    {
        var reservation = await ctx.Reservations
            .Include(x => x.PaymentReservations)
                .ThenInclude(x => x.Payment)
                    .ThenInclude(x => x.PaymentReservations)
                        .ThenInclude(x => x.Reservation)
            .FirstOrDefaultAsync(
                x => x.Id == request.Id &&
                     !x.IsDeleted,
                ct);

        if (reservation is null)
            throw new MarketNotFoundException("Reservation not found.");

        // Only confirmed reservations may be edited.
        if (reservation.Status != ReservationStatus.Confirmed)
        {
            throw new MarketBusinessRuleException(
                "RESERVATION_NOT_CONFIRMED",
                "Only confirmed reservations can be updated.");
        }

        // Do not allow update after payment is completed
        if (reservation.PaymentStatus == PaymentStatus.Paid)
        {
            throw new MarketBusinessRuleException(
                "RESERVATION_PAID",
                "Paid reservations cannot be updated.");
        }

        // Current reservation start must be at least 2 hours in the future
        var currentReservationStart =
            reservation.Date.Date.Add(reservation.StartTime.TimeOfDay);

        if (currentReservationStart - businessClock.Now < TimeSpan.FromHours(2))
        {
            throw new MarketBusinessRuleException(
                "RESERVATION_TOO_CLOSE",
                "Reservation can only be updated at least 2 hours before the start time.");
        }

   


        var rulesResult =
              await reservationRulesService
                  .ValidateAsync(
                      new ReservationRulesRequest(
                          request.ReservationDate,
                          request.StartTime,
                          request.EndTime,
                          request.TrackId,
                          request.KartId,
                          ExcludeReservationId:
                              request.Id),
                      pendingSlots: null,
                      ct);

       
        reservation.TrackId = request.TrackId;
        reservation.KartId = request.KartId;
        reservation.Date = request.ReservationDate.Date;
        reservation.StartTime = request.StartTime;
        reservation.EndTime = request.EndTime;
        reservation.TotalPrice = rulesResult.TotalPrice;

        RecalculatePayments(reservation);

        await ctx.SaveChangesAsync(ct);

        return reservation.Id;
    }
    private static void RecalculatePayments(
       ReservationEntity reservation)
    {
        foreach (var paymentReservation
                 in reservation.PaymentReservations)
        {
            var payment =
                paymentReservation.Payment;

            if (payment.Status ==
                PaymentStatus.Paid)
            {
                continue;
            }

            payment.Amount =
                payment.PaymentReservations
                    .Where(x =>
                        !x.Reservation.IsDeleted)
                    .Sum(x =>
                        x.Reservation.TotalPrice);
        }
    }
}