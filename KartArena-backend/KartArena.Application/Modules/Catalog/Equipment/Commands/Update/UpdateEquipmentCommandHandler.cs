namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Update;

public sealed class UpdateEquipmentCommandHandler(IAppDbContext ctx)
    : IRequestHandler<UpdateEquipmentCommand, Unit>
{
    public async Task<Unit> Handle(UpdateEquipmentCommand request, CancellationToken ct)
    {
        var entity = await ctx.EquipmentEntity
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (entity is null)
            throw new MarketNotFoundException($"Oprema (ID={request.Id}) nije pronađena.");

        var nextName = request.Name?.Trim() ?? entity.Name;
        var nextSize = request.Size?.Trim() ?? entity.Size;
        var nextCategory = request.Category ?? entity.Category;

        var exists = await ctx.EquipmentEntity
            .AnyAsync(x => x.Id != request.Id
                        && x.Name == nextName
                        && x.Size == nextSize
                        && x.Category == nextCategory,
                ct);

        if (exists)
            throw new MarketConflictException("Oprema za odabranu kategoriju i veličinu već postoji.");

        entity.Name = nextName;
        entity.Category = nextCategory;
        entity.Price = request.Price ?? entity.Price;
        entity.Size = nextSize;
        entity.Description = request.Description is null ? entity.Description : request.Description.Trim();

        if (request.IsActive.HasValue)
            entity.isEnabled = request.IsActive.Value;

        await ctx.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
