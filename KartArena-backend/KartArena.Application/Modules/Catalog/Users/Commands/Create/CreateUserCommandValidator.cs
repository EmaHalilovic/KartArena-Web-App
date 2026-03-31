using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Users.Commands.Create
{
    public sealed class CreateUserCommandValidator
        : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(x => x.FirstName)
                 .NotEmpty()
                 .WithMessage("First name is required.")
                 .MaximumLength(50)
                 .WithMessage("First name can be at most 50 characters long.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.")
                .MaximumLength(50)
                .WithMessage("Last name can be at most 50 characters long.");

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username is required.")
                .MinimumLength(4)
                .WithMessage("Username must be at least 4 characters long.")
                .MaximumLength(50)
                .WithMessage("Username can be at most 50 characters long.");

            RuleFor(x => x.Email)
     .NotEmpty()
     .WithMessage("Email is required.")
     .EmailAddress()
     .WithMessage("Please enter a valid email address.");


            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters long.");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty()
                .WithMessage("Date of birth is required.")
                .LessThan(DateTime.Today)
                .WithMessage("Date of birth must be in the past.");

            RuleFor(x => x.CityId)
                .GreaterThan(0)
                .WithMessage("A valid city must be selected.");

            RuleFor(x => x.RoleId)
                .GreaterThan(0)
                .WithMessage("A valid role must be selected.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .WithMessage("Phone number can be at most 20 characters long.")
                .Matches(@"^\+\d{1,3}\d{6,12}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Phone number contains invalid characters.");

            RuleFor(x => x.Gender)
                .Must(x =>
                    string.IsNullOrEmpty(x) ||
                    x == "M" ||
                    x == "F" ||
                    x == "Other")
                .WithMessage("Gender must be 'M', 'F', or 'Other'.");

            RuleFor(x => x.Address)
                .MaximumLength(100)
                .WithMessage("Address can be at most 100 characters long.");
        }
    }
}
