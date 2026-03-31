namespace KartArena.Application.Modules.Catalog.Users.Commands.Status.Disable
{
    public sealed class DisableUserCommandHandler(IAppDbContext context)
       : IRequestHandler<DisableUserCommand, Unit>
    {
        public async Task<Unit> Handle(DisableUserCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (user is null)
                throw new Exception($"User with ID {request.Id} was not found.");

            if (!user.isEnabled)
                return Unit.Value;

            user.isEnabled = false;

            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
