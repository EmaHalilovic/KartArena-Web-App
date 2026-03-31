namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Delete;

public class DeleteEquipmentCommandHandler(IAppDbContext context)
      : IRequestHandler<DeleteEquipmentCommand, Unit>
{
    public async Task<Unit> Handle(DeleteEquipmentCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.EquipmentEntity
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
            throw new MarketNotFoundException("Oprema nije pronađena.");

        if (entity.Items.Any())
            throw new MarketBusinessRuleException("124", "Oprema ima povezane stavke i nije moguće obrisati tip opreme.");

        entity.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
