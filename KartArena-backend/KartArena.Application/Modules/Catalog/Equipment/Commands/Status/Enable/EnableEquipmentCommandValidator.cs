namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Status.Enable;

public sealed class EnableEquipmentCommandValidator : AbstractValidator<EnableEquipmentCommand>
{
    public EnableEquipmentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
