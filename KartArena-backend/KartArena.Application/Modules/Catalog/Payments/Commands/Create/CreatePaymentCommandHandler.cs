using KartArena.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Payments.Commands.Create;

public sealed class CreatePaymentCommandHandler(IAppDbContext context)
    : IRequestHandler<CreatePaymentCommand, int>
{
    public async Task<int> Handle(
        CreatePaymentCommand request,
        CancellationToken cancellationToken)
    {
        var reservation = await context.Reservations
            .SingleOrDefaultAsync(
                x => x.Id == request.ReservationId &&
                     !x.IsDeleted,
                cancellationToken);

        if (reservation is null)
        {
            throw new MarketNotFoundException(
                $"Reservation with id {request.ReservationId} was not found.");
        }

        var paymentType = await context.PaymentTypes
            .SingleOrDefaultAsync(
                x => x.Id == request.PaymentTypeId &&
                     !x.IsDeleted,
                cancellationToken);

        if (paymentType is null)
        {
            throw new MarketNotFoundException(
                $"Payment type with id {request.PaymentTypeId} was not found.");
        }

        if (!paymentType.isEnabled)
        {
            throw new MarketBusinessRuleException(
                "payment.create.payment_type_disabled",
                "Selected payment type is disabled.");
        }

        var alreadyHasActivePayment =
            await context.PaymentReservations
                .AnyAsync(
                    link =>
                        link.ReservationId == request.ReservationId &&
                        !link.Payment.IsDeleted &&
                        link.Payment.Status != PaymentStatus.Cancelled &&
                        link.Payment.Status != PaymentStatus.Failed,
                    cancellationToken);

        if (alreadyHasActivePayment)
        {
            throw new MarketBusinessRuleException(
                "payment.create.reservation_already_has_payment",
                "The reservation already has an active payment.");
        }

        var payment = new PaymentEntity
        {
            PaymentTypeId = request.PaymentTypeId,
            Amount = request.Amount,
            Currency = "bam",
            PaymentDate = request.PaymentDate,
            TransactionReference =
                request.TransactionReference?.Trim(),
            Note = request.Note?.Trim(),
            Status = PaymentStatus.Pending,
            isEnabled = true
        };

        var paymentReservation =
            new PaymentReservationEntity
            {
                Payment = payment,
                Reservation = reservation
            };

        context.Payments.Add(payment);
        context.PaymentReservations.Add(paymentReservation);

        reservation.PaymentStatus =
            PaymentStatus.Pending;

        await context.SaveChangesAsync(
            cancellationToken);

        return payment.Id;
    }
}