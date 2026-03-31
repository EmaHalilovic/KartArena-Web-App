namespace KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Status.Disable;

public sealed class DisablePaymentTypeCommandHandler(IAppDbContext context)
    : IRequestHandler<DisablePaymentTypeCommand, Unit>
{
    public async Task<Unit> Handle(DisablePaymentTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.PaymentTypes
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null || entity.IsDeleted)
            throw new MarketNotFoundException("Payment type was not found.");

        if (!entity.isEnabled)
            return Unit.Value;

        var hasActivePayments = await context.Payments
            .AnyAsync(x => x.PaymentTypeId == request.Id && x.isEnabled, cancellationToken);

        if (hasActivePayments)
            throw new MarketBusinessRuleException("paymenttype.disable.blocked", "Cannot disable payment type while active payments use it.");

        entity.isEnabled = false;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
