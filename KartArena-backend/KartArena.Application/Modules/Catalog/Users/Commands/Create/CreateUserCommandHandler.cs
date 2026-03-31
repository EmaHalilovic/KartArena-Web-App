using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Users.Commands.Create
{
    public sealed class CreateUserCommandHandler(IAppDbContext context,IPasswordHasher<UserEntity>passwordHasher)
        : IRequestHandler<CreateUserCommand, int>
    {
        public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            //provjeri da li email postoji
            var emailExists = await context.Users.AnyAsync
                (x => x.Email == request.Email, cancellationToken);
            if (emailExists)
            {
                throw new InvalidOperationException($"The user with: {request.Email} already exists.");
            }

            //provjeri da li grad postoji
            var cityExists = await context.Cities.AnyAsync
                (x => x.Id == request.CityId, cancellationToken);
            if (!cityExists)
            {
                throw new KeyNotFoundException($"The city with id: {request.CityId} does not exist.");
            }

            // provjeri da li username postoji
            var usernameExists = await context.Users.AnyAsync(
                x => x.Username.ToLower() == request.Username.ToLower() && !x.IsDeleted,
                cancellationToken);

            if (usernameExists)
            {
                throw new InvalidOperationException(
                    $"The username: {request.Username} is already taken.");
            }
            var user = new UserEntity
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                PhoneNumber = request.PhoneNumber,
                Gender = request.Gender,
                Address = request.Address,
                Image = request.Image,

                Username = request.Username,
                Email = request.Email,

                CityId = request.CityId,
                RoleId = request.RoleId,

                isEnabled = true,
                IsDeleted = false,
                CreatedAtUtc = DateTime.UtcNow
            };
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);

            return user.Id;
        }
    }
}