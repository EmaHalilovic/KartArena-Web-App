using FluentValidation;

namespace KartArena.Application.Modules.Catalog.Karts.Commands.Update
{
    public sealed class UpdateKartCommandValidator
        : AbstractValidator<UpdateKartCommand>
    {
        public UpdateKartCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Id must be greater than 0.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name can be at most 100 characters long.");

            RuleFor(x => x.ChassisNumber)
                .MaximumLength(50).WithMessage("Chassis number can be at most 50 characters long.");

            RuleFor(x => x.Manufacturer)
                .MaximumLength(100).WithMessage("Manufacturer can be at most 100 characters long.");

            RuleFor(x => x.Colour)
                .MaximumLength(50).WithMessage("Colour can be at most 50 characters long.");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).WithMessage("ImageUrl can be at most 500 characters long.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description can be at most 2000 characters long.");

            RuleFor(x => x.YearOfManufacture)
                .GreaterThan(1990).When(x => x.YearOfManufacture.HasValue)
                .WithMessage("Year of manufacture must be after 1990.")
                .LessThanOrEqualTo(DateTime.Now.Year)
                .When(x => x.YearOfManufacture.HasValue)
                .WithMessage("Year of manufacture cannot be in the future.");
        }
    }
}
