using KartArena.Application.Modules.Catalog.Equipment.Commands.Delete;
using KartArena.Application.Modules.Catalog.EquipmentItems.Commands.Delete;

namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Update;

public sealed class DeleteEquipmentItemCommandValidator : AbstractValidator<DeleteEquipmentItemCommand>
{
    public DeleteEquipmentItemCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        
    }
}
