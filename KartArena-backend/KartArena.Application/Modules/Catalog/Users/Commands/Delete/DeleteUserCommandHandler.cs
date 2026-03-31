namespace KartArena.Application.Modules.Catalog.Users.Commands.Delete
{
    public sealed class DeleteUserCommandHandler(IAppDbContext context)
        : IRequestHandler<DeleteUserCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (user is null)
                throw new Exception($"User with ID {request.Id} was not found.");

            user.IsDeleted = true;
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
