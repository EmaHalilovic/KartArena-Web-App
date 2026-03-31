namespace KartArena.Application.Modules.Catalog.Users.Commands.Update
{
    public sealed class UpdateUserCommandHandler(IAppDbContext context)
         : IRequestHandler<UpdateUserCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (user == null)
                throw new Exception($"User with ID {request.Id} was not found.");

            var cityExists = await context.Cities
                .AnyAsync(x => x.Id == request.CityId, cancellationToken);

            if (!cityExists)
                throw new KeyNotFoundException($"City with ID {request.CityId} does not exist.");
            
            // Update properties
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.DateOfBirth = request.DateOfBirth;
            user.PhoneNumber = request.PhoneNumber;
            user.Gender = request.Gender;
            user.Address = request.Address;
            user.Email = request.Email;
            user.CityId = request.CityId;
            user.Image = request.Image;

            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
