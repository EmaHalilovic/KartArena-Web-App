using FluentValidation;

namespace KartArena.Application.Modules.Catalog.Karts.Queries.GetAll
{
    public sealed class GetAllKartsQueryValidator
        : AbstractValidator<GetAllKartsQuery>
    {
        public GetAllKartsQueryValidator()
        {
            RuleFor(x => x.Search)
                .MaximumLength(100)
                .WithMessage("Search term can be at most 100 characters long.");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");
        }
    }
}
