using FluentValidation;
using KartArena.Domain.Entities.Catalog;

namespace KartArena.Application.Modules.Catalog.Karts.Commands.Create
{
    public sealed class CreateKartCommandValidator
        : AbstractValidator<CreateKartCommand>
    {
        public CreateKartCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name can be at most 100 characters long.");

            RuleFor(x => x.ChassisNumber)
                .NotEmpty().WithMessage("Chassis number is required.")
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
                .GreaterThan(1990).WithMessage("Year of manufacture must be after 1990.")
                .LessThanOrEqualTo(DateTime.Now.Year).WithMessage("Year of manufacture cannot be in the future.");
        }
    }
}
