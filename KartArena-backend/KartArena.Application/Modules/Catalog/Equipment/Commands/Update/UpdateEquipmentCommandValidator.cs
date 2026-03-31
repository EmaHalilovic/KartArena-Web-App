namespace KartArena.Application.Modules.Catalog.Equipment.Commands.Update;

public sealed class UpdateEquipmentCommandValidator : AbstractValidator<UpdateEquipmentCommand>
{
    public UpdateEquipmentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);

        When(x => x.Name is not null, () =>
        {
            RuleFor(x => x.Name!).NotEmpty().MaximumLength(100);
        });

        When(x => x.Size is not null, () =>
        {
            RuleFor(x => x.Size!).NotEmpty().MaximumLength(20);
        });

        When(x => x.Price.HasValue, () =>
        {
            RuleFor(x => x.Price!.Value).GreaterThanOrEqualTo(0);
        });
    }
}
