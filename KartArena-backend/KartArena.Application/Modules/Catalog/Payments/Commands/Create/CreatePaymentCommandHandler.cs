using KartArena.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Payments.Commands.Create;

public sealed class CreatePaymentCommandHandler(IAppDbContext context)
    : IRequestHandler<CreatePaymentCommand, int>
{
    public async Task<int> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var reservationExists = await context.Reservations
            .AnyAsync(x => x.Id == request.ReservationId && !x.IsDeleted, cancellationToken);

        if (!reservationExists)
            throw new MarketNotFoundException($"Reservation with id {request.ReservationId} was not found.");

       
            var paymentType = await context.PaymentTypes
                .FirstOrDefaultAsync(x => x.Id == request.PaymentTypeId && !x.IsDeleted, cancellationToken);

            if (paymentType is null)
                throw new MarketNotFoundException($"Payment type with id {request.PaymentTypeId} was not found.");

            if (!paymentType.isEnabled)
                throw new MarketBusinessRuleException(
                    "payment.create.payment_type_disabled",
                    "Selected payment type is disabled.");
        

        var entity = new PaymentEntity
        {
            ReservationId = request.ReservationId,
            PaymentTypeId = request.PaymentTypeId,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate ?? DateTime.UtcNow,
            TransactionReference = request.TransactionReference?.Trim(),
            Note = request.Note?.Trim(),
            Status = PaymentStatus.Pending,
            isEnabled=true
        };

        context.Payments.Add(entity);

        await context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}