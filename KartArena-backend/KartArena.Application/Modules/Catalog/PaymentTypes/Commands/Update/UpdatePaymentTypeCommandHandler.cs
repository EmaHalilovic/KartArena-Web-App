using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Update;

public sealed class UpdatePaymentTypeCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdatePaymentTypeCommand, Unit>
{
    public async Task<Unit> Handle(UpdatePaymentTypeCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.PaymentTypes
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null || entity.IsDeleted)
            throw new MarketNotFoundException("Payment type was not found.");

        var normalizedName = request.Name.Trim();

        var exists = await context.PaymentTypes
            .AnyAsync(
                x => x.Id != request.Id &&
                     x.Name != null &&
                     x.Name.ToLower() == normalizedName.ToLower(),
                cancellationToken);

        if (exists)
            throw new MarketConflictException("Payment type with the same name already exists.");

        entity.Name = normalizedName;
        entity.AllowedOnline = request.AllowedOnline;
        entity.AllowedAtDesk = request.AllowedAtDesk;
        entity.Description = request.Description?.Trim();

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}