namespace KartArena.Application.Modules.Catalog.Equipment.Queries.GetById;

public sealed class GetEquipmentByIdQueryValidator : AbstractValidator<GetEquipmentByIdQuery>
{
    public GetEquipmentByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id must be a positive value.");
    }
}
