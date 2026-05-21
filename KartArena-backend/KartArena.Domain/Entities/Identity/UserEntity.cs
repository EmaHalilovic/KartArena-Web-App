using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using KartArena.Domain.Common;
using KartArena.Domain.Entities.Catalog;
using KartArena.Domain.Entities.Reservations;

namespace KartArena.Domain.Entities.Identity
{
    public sealed class UserEntity : BaseEntity
    {
        // LOGIN / AUTH
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public bool IsEmailConfirmed { get; set; }
        public DateTime? LastLoginAtUtc { get; set; }
        public int FailedLoginAttempts { get; set; }

        [NotMapped]
        public bool IsLockedOut => FailedLoginAttempts >= 5;

        // ROLE
        public int RoleId { get; set; }
        public RoleEntity Role { get; set; } = default!;

        // PROFILE
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public DateTime DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public byte[]? Image { get; set; }

        // KARTING DATA
        public int TotalRaces { get; set; }
        public TimeSpan? BestLapTime { get; set; }

        // LOCATION
        public int CityId { get; set; }
        public CityEntity City { get; set; } = default!;

        // COLLECTIONS
        public IReadOnlyCollection<RaceUserEntity>? Races { get; set; }
        public IReadOnlyCollection<ServiceEntity>? Services { get; set; }
        public IReadOnlyCollection<ReviewEntity>? Reviews { get; set; }
        public IReadOnlyCollection<NotificationEntity>? Notifications { get; set; }
        public IReadOnlyCollection<ReservationEmployeeEntity>? ReservationEmployees { get; set; }
        public IReadOnlyCollection<RefreshTokenEntity>? RefreshTokens { get; set; }
    }

}