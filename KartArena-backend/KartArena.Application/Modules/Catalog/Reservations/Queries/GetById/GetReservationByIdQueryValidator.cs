using FluentValidation;

namespace KartArena.Application.Modules.Catalog.Reservations.Queries.GetById
{
    public sealed class GetReservationByIdQueryValidator : AbstractValidator<GetReservationByIdQuery>
    {
        public GetReservationByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Id must be a positive value.");
        }
    }
}