using KartArena.Domain.Entities.Equipment;
using KartArena.Domain.Entities.Identity;
using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace KartArena.Application.Abstractions;

// Application layer
public interface IAppDbContext
{
    DbSet<UserEntity> Users { get; }
    DbSet<CityEntity> Cities { get; }
    DbSet<RefreshTokenEntity> RefreshTokens { get; }
    //payments
    DbSet<PaymentTypeEntity> PaymentTypes { get; }
    DbSet<PaymentEntity> Payments { get; }

    DbSet<ReservationEntity> Reservations { get; }
    DbSet<EquipmentTypeEntity> EquipmentEntity { get; }
    DbSet<EquipmentItemEntity> EquipmentItemEntity { get; }
    DbSet<ReservationEmployeeEntity> ReservationEmployees { get; }
    //karts
    DbSet<KartEntity> Karts { get; }
    DbSet<TrackEntity> Tracks { get; }

    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken ct);
}