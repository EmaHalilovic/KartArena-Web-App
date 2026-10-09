using KartArena.Application.Modules.Catalog.Reservations.Services;
using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Create;

public sealed class CreateReservationCommandHandler(
    IAppDbContext ctx,
    IReservationRulesService reservationRulesService)
    : IRequestHandler<CreateReservationCommand, int>
{
   
    private const string DeskCashPaymentCode = "DESK_CASH";
    private const string DefaultCurrency = "bam";

    public async Task<int> Handle(
        CreateReservationCommand request,
        CancellationToken ct)
    {
       
       

        if (request.PaymentTypeId is not int paymentTypeId)
        {
            throw new MarketBusinessRuleException(
                "PAYMENT_TYPE_REQUIRED",
                "Please select a payment type.");
        }

        var paymentType = await ctx.PaymentTypes
            .FirstOrDefaultAsync(
                x => x.Id == paymentTypeId && !x.IsDeleted,
                ct);

        if (paymentType is null)
        {
            throw new MarketBusinessRuleException(
                "PAYMENT_TYPE_NOT_FOUND",
                "Selected payment type does not exist.");
        }

        if (!paymentType.isEnabled)
        {
            throw new MarketBusinessRuleException(
                "PAYMENT_TYPE_DISABLED",
                "Selected payment type is not active.");
        }

        if (!paymentType.AllowedOnline)
        {
            throw new MarketBusinessRuleException(
                "PAYMENT_TYPE_NOT_ALLOWED",
                "Selected payment type is not available for online booking.");
        }

        var isDeskCashPayment = string.Equals(
            paymentType.Code,
            DeskCashPaymentCode,
            StringComparison.OrdinalIgnoreCase);

        var rulesResult =
            await reservationRulesService.ValidateAsync(
                new ReservationRulesRequest(
                    request.ReservationDate,
                    request.StartTime,
                    request.EndTime,
                    request.TrackId,
                    request.KartId),
                pendingSlots: null,
                ct);

        var reservation = new ReservationEntity
        {
           

            CustomerFirstName =
                request.CustomerFirstName.Trim(),

            CustomerLastName =
                request.CustomerLastName.Trim(),

            CustomerEmail =
                request.CustomerEmail.Trim(),

            CustomerPhone =
                request.CustomerPhone.Trim(),


            TrackId = request.TrackId,
            KartId = request.KartId,

            Date = request.ReservationDate.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,

            Status = isDeskCashPayment
                ? ReservationStatus.Confirmed
                : ReservationStatus.Pending,

            PaymentStatus = PaymentStatus.Pending,

            TotalPrice = rulesResult.TotalPrice,

            IsDeleted = false
        };

        await ctx.Reservations.AddAsync(
            reservation,
            ct);

        if (paymentType is not null)
        {
            var payment = new PaymentEntity
            {
                PaymentTypeId = paymentType.Id,

                Amount = rulesResult.TotalPrice,
                Currency = DefaultCurrency,

                Status = PaymentStatus.Pending,
                PaymentDate = null,

                Note = string.IsNullOrWhiteSpace(
                request.PaymentNote)
                ? null
                : request.PaymentNote.Trim(),

                IsDeleted = false,
                isEnabled = true
            };

            var paymentReservation =
                new PaymentReservationEntity
                {
                    Payment = payment,
                    Reservation = reservation
                };

            await ctx.Payments.AddAsync(
                payment,
                ct);

            await ctx.PaymentReservations.AddAsync(
                paymentReservation,
                ct);
        }

        await ctx.SaveChangesAsync(ct);

        return reservation.Id;
    }
}
