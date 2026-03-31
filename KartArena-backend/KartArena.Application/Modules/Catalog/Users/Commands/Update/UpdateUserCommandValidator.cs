namespace KartArena.Application.Modules.Catalog.Users.Commands.Update
{
    public sealed class UpdateUserCommandValidator
        : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.FirstName)
                 .NotEmpty().WithMessage("First name is required.")
                 .MaximumLength(50).WithMessage("First name can be at most 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name can be at most 50 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past.");

            RuleFor(x => x.CityId)
                .GreaterThan(0).WithMessage("City must be selected.");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+\d{1,3}\d{6,12}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Phone number must be in format +[country code][number].");
        }
    }
}
