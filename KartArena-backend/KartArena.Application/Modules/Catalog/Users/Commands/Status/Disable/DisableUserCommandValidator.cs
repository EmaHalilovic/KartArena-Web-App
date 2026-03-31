namespace KartArena.Application.Modules.Catalog.Users.Commands.Status.Disable
{
    public sealed class DisableUserCommandValidator : AbstractValidator<DisableUserCommand>
    {
        public DisableUserCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("User ID must be greater than 0.");
        }

    }
}
