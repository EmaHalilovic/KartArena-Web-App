using FluentValidation;

namespace KartArena.Application.Modules.Catalog.Karts.Queries.List
{
    public sealed class ListKartsQueryValidator
        : AbstractValidator<ListKartsQuery>
    {
        public ListKartsQueryValidator()
        {
            RuleFor(x => x.Paging.Page)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0.");

            RuleFor(x => x.Paging.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100.");

            RuleFor(x => x.Search)
                .MaximumLength(100)
                .WithMessage("Search term can be at most 100 characters long.");
        }
    }
}
