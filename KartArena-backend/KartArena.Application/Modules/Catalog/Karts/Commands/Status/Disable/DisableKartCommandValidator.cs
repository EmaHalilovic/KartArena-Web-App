using FluentValidation;

namespace KartArena.Application.Modules.Catalog.Karts.Commands.Status.Disable
{
    public sealed class DisableKartCommandValidator
        : AbstractValidator<DisableKartCommand>
    {
        public DisableKartCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Id must be greater than 0.");
        }
    }
}
