using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Delete;

public sealed class DeletePaymentTypeCommandHandler(IAppDbContext context)
    : IRequestHandler<DeletePaymentTypeCommand, Unit>
{
    public async Task<Unit> Handle(DeletePaymentTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.PaymentTypes
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null || entity.IsDeleted)
            throw new MarketNotFoundException("Payment type was not found.");

        var hasPayments = await context.Payments
            .AnyAsync(x => x.PaymentTypeId == request.Id && !x.IsDeleted, cancellationToken);

        if (hasPayments)
            throw new MarketBusinessRuleException(
                "payment_type.delete.blocked",
                "Cannot delete payment type that is already used by payments.");

        entity.IsDeleted = true;

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}