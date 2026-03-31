using FluentValidation;

namespace KartArena.Application.Modules.Catalog.Karts.Queries.GetById
{
    public sealed class GetKartByIdQueryValidator
        : AbstractValidator<GetKartByIdQuery>
    {
        public GetKartByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Id must be greater than 0.");
        }
    }
}
