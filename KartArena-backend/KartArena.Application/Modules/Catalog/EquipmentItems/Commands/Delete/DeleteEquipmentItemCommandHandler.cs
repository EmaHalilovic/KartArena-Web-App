namespace KartArena.Application.Modules.Catalog.EquipmentItems.Commands.Delete;

public sealed class DeleteEquipmentItemCommandHandler(IAppDbContext context)
    : IRequestHandler<DeleteEquipmentItemCommand, Unit>
{
    public async Task<Unit> Handle(DeleteEquipmentItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.EquipmentItemEntity
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new MarketNotFoundException("Equipment item was not found.");

        entity.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
