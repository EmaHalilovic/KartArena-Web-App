namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Status.Disable;

public sealed class DisableEquipmentCommandValidator : AbstractValidator<DisableEquipmentCommand>
{
    public DisableEquipmentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
