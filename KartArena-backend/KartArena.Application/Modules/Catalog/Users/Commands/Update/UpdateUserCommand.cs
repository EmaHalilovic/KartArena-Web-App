using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using System.Text.Json.Serialization;

namespace KartArena.Application.Modules.Catalog.Users.Commands.Update
{
    public sealed class UpdateUserCommand : IRequest<Unit>
    {
        [JsonIgnore]
        public int Id { get; set; }
        // PROFILE
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public byte[]? Image { get; set; }

        // LOGIN
        public string Email { get; set; } = default!;

        // ROLE (admin edit)
        public int RoleId { get; set; }

        // LOCATION
        public int CityId { get; set; }

    }
}
