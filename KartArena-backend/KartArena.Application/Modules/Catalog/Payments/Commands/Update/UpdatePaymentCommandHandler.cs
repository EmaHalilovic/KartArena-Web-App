using KartArena.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Payments.Commands.Update;

public sealed class UpdatePaymentCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdatePaymentCommand, Unit>
{
    public async Task<Unit> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Payments
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (entity is null)
            throw new MarketNotFoundException("Payment was not found.");

        if (entity.Status == PaymentStatus.Refunded)
            throw new MarketBusinessRuleException(
                "payment.update.blocked",
                "Refunded payment cannot be updated.");

       
            var paymentType = await context.PaymentTypes
                .FirstOrDefaultAsync(x => x.Id == request.PaymentTypeId && !x.IsDeleted, cancellationToken);

            if (paymentType is null)
                throw new MarketNotFoundException($"Payment type with id {request.PaymentTypeId} was not found.");

            if (!paymentType.isEnabled)
                throw new MarketBusinessRuleException(
                    "payment.update.payment_type_disabled",
                    "Selected payment type is disabled.");
        

        entity.Amount = request.Amount;
        entity.PaymentDate = request.PaymentDate ?? entity.PaymentDate ?? DateTime.UtcNow;
        entity.PaymentTypeId = request.PaymentTypeId;
        entity.TransactionReference = request.TransactionReference?.Trim();
        entity.Note = request.Note?.Trim();

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}