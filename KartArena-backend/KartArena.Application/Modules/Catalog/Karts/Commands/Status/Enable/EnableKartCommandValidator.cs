using FluentValidation;

namespace KartArena.Application.Modules.Catalog.Karts.Commands.Status.Enable
{
    public sealed class EnableKartCommandValidator
        : AbstractValidator<EnableKartCommand>
    {
        public EnableKartCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Id must be greater than 0.");
        }
    }
}
