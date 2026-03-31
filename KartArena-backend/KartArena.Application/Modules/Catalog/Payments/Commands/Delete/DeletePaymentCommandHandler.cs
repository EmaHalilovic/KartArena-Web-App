using KartArena.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Payments.Commands.Delete;

public sealed class DeletePaymentCommandHandler(IAppDbContext context)
    : IRequestHandler<DeletePaymentCommand, Unit>
{
    public async Task<Unit> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Payments
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (entity is null)
            throw new MarketNotFoundException("Payment was not found.");

        if (entity.Status == PaymentStatus.Paid)
            throw new MarketBusinessRuleException(
                "payment.delete.blocked",
                "Paid payment cannot be deleted.");

        if (entity.Status == PaymentStatus.Refunded)
            throw new MarketBusinessRuleException(
                "payment.delete.blocked",
                "Refunded payment cannot be deleted.");

        entity.IsDeleted = true;

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}