namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Status.Enable;

public sealed class EnableEquipmentCommandHandler(IAppDbContext ctx)
    : IRequestHandler<EnableEquipmentCommand, Unit>
{
    public async Task<Unit> Handle(EnableEquipmentCommand request, CancellationToken ct)
    {
        var entity = await ctx.EquipmentEntity
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (entity is null)
            throw new MarketNotFoundException($"Oprema (ID={request.Id}) nije pronađena.");

        if (entity.isEnabled)
            return Unit.Value;

        entity.isEnabled = true;

        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
