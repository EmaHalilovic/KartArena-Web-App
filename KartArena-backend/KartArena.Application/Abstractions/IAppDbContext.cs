using KartArena.Domain.Entities.Equipment;
using KartArena.Domain.Entities.Identity;
using KartArena.Domain.Entities.Payments;

namespace KartArena.Application.Abstractions;

// Application layer
public interface IAppDbContext
{
    DbSet<UserEntity> Users { get; }
    DbSet<CityEntity>  Cities { get; }
    DbSet<RefreshTokenEntity> RefreshTokens { get; }
    //payments
    DbSet<PaymentTypeEntity> PaymentTypes { get; }
    DbSet<PaymentEntity> Payments { get; }

    DbSet<ReservationEntity> Reservations { get; }
    DbSet<EquipmentTypeEntity> EquipmentEntity { get; }
    DbSet<EquipmentItemEntity> EquipmentItemEntity { get; }

    //karts
    DbSet<KartEntity> Karts { get; }
    DbSet<TrackEntity> Tracks { get; }

    Task<int> SaveChangesAsync(CancellationToken ct);
}