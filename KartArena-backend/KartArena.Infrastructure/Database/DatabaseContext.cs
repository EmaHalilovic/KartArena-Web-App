using KartArena.Application.Abstractions;
using KartArena.Domain.Entities.Equipment;
using KartArena.Domain.Entities.Identity;
using KartArena.Domain.Entities.Payments;

namespace KartArena.Infrastructure.Database;

public partial class DatabaseContext : DbContext, IAppDbContext
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<CityEntity> Cities => Set<CityEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    public DbSet<PowertrainTypeEntity>PowertrainTypes=> Set<PowertrainTypeEntity>();
    public DbSet<RoleEntity> Roles=> Set<RoleEntity>();

    //payment
    public DbSet<PaymentTypeEntity> PaymentTypes => Set<PaymentTypeEntity>();
    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();

    public DbSet<ReservationEntity> Reservations => Set<ReservationEntity>();

    public DbSet<EquipmentTypeEntity> EquipmentEntity => Set<EquipmentTypeEntity>();

    public DbSet<EquipmentItemEntity> EquipmentItemEntity => Set<EquipmentItemEntity>();

    //kart
    public DbSet<KartEntity> Karts => Set<KartEntity>();
    public DbSet<TrackEntity> Tracks => Set<TrackEntity>();


    private readonly TimeProvider _clock;
    public DatabaseContext(DbContextOptions<DatabaseContext> options, TimeProvider clock) : base(options)
    {
        _clock = TimeProvider.System;
    }
}