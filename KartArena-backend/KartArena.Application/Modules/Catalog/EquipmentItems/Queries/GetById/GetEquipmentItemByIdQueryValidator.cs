namespace KartArena.Application.Modules.Catalog.EquipmentItems.Queries.GetById;

public sealed class GetEquipmentItemByIdQueryValidator : AbstractValidator<GetEquipmentItemByIdQuery>
{
    public GetEquipmentItemByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be a positive value.");
    }
}
