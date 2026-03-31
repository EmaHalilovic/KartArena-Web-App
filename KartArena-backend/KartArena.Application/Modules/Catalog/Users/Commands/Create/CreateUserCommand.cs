using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Users.Commands.Create
{
    public sealed class CreateUserCommand : IRequest<int>
    {
        // PROFILE
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public byte[]? Image { get; set; }

        // LOGIN
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;

        // ROLE
        public int RoleId { get; set; }

        // LOCATION
        public int CityId { get; set; }
    }

}

