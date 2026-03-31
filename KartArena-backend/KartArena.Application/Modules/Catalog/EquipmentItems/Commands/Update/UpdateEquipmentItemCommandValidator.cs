namespace KartArena.Application.Modules.Catalog.EquipmentItems.Commands.Update;

public sealed class UpdateEquipmentItemCommandValidator
    : AbstractValidator<UpdateEquipmentItemCommand>
{
    public UpdateEquipmentItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(x => x.ItemCode)
            .MaximumLength(50).When(x => x.ItemCode is not null)
            .WithMessage("Item code must not exceed 50 characters.");

        RuleFor(x => x.EquipmentTypeId)
            .GreaterThan(0).When(x => x.EquipmentTypeId.HasValue)
            .WithMessage("Equipment type id must be greater than 0.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).When(x => x.Notes is not null)
            .WithMessage("Notes must not exceed 500 characters.");
    }
}
