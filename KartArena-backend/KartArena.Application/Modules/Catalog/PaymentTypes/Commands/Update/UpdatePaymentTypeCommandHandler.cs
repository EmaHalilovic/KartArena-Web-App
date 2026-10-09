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

        var NameExists = await context.PaymentTypes
            .AnyAsync(
                x => x.Id != request.Id &&
                     x.Name != null &&
                     x.Name.ToLower() == normalizedName.ToLower(),
                cancellationToken);

        if (NameExists)
            throw new MarketConflictException("Payment type with the same name already exists.");

        var normalizedCode = request.Code.Trim();

        var CodeExists = await context.PaymentTypes
            .AnyAsync(
                x => x.Id != request.Id &&
                     x.Code != null &&
                     x.Code.ToLower() == normalizedCode.ToLower(),
                cancellationToken);

        if (CodeExists)
            throw new MarketConflictException("Payment type with the same code already exists.");

        entity.Name = normalizedName;
        entity.Code = normalizedCode;
        entity.AllowedOnline = request.AllowedOnline;
        entity.AllowedAtDesk = request.AllowedAtDesk;
        entity.Description = request.Description?.Trim();

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}