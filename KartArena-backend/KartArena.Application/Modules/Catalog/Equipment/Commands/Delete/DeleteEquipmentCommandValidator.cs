using KartArena.Application.Modules.Catalog.Equipment.Commands.Delete;

namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Update;

public sealed class DeleteEquipmentCommandValidator : AbstractValidator<DeleteEquipmentCommand>
{
    public DeleteEquipmentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        
    }
}
