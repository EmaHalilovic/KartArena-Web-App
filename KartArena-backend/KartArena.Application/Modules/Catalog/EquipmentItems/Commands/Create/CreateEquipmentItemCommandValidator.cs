namespace KartArena.Application.Modules.Catalog.EquipmentItems.Commands.Create;

public sealed class CreateEquipmentItemCommandValidator
    : AbstractValidator<CreateEquipmentItemCommand>
{
    public CreateEquipmentItemCommandValidator()
    {
        RuleFor(x => x.ItemCode)
            .NotEmpty().WithMessage("Item code is required.")
            .MaximumLength(50).WithMessage("Item code must not exceed 50 characters.");

        RuleFor(x => x.EquipmentTypeId)
            .GreaterThan(0).WithMessage("Equipment type is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes must not exceed 500 characters.");
    }
}
