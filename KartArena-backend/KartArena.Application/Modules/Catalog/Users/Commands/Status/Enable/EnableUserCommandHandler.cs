namespace KartArena.Application.Modules.Catalog.Users.Commands.Status.Enable
{
    public sealed class EnableUserCommandHandler(IAppDbContext context)
        : IRequestHandler<EnableUserCommand, Unit>
    {
        public async Task<Unit> Handle(EnableUserCommand request, CancellationToken cancellationToken)
        {

            var user = await context.Users
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (user == null)
                throw new Exception($"User with ID {request.Id} was not found.");

            if (!user.isEnabled)
            {
                user.isEnabled = true;
                await context.SaveChangesAsync(cancellationToken);

            }

            return Unit.Value;
        }
    }
}
