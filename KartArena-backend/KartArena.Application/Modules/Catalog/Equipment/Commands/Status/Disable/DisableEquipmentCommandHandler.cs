namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Status.Disable;

public sealed class DisableEquipmentCommandHandler(IAppDbContext ctx)
    : IRequestHandler<DisableEquipmentCommand, Unit>
{
    public async Task<Unit> Handle(DisableEquipmentCommand request, CancellationToken ct)
    {
        var entity = await ctx.EquipmentEntity
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (entity is null)
            throw new MarketNotFoundException($"Oprema (ID={request.Id}) nije pronađena.");

        if (!entity.isEnabled)
            return Unit.Value;

        entity.isEnabled = false;

        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
