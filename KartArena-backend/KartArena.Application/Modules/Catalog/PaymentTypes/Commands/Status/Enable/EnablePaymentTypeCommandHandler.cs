namespace KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Status.Enable;

public sealed class EnablePaymentTypeCommandHandler(IAppDbContext context)
    : IRequestHandler<EnablePaymentTypeCommand, Unit>
{
    public async Task<Unit> Handle(EnablePaymentTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.PaymentTypes
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null || entity.IsDeleted)
            throw new MarketNotFoundException("Payment type was not found.");

        if (!entity.isEnabled)
        {
            entity.isEnabled = true;
            await context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
