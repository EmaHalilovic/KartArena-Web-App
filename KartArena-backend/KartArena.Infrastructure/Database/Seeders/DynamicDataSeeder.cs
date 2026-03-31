using KartArena.Domain.Entities.Catalog;
using KartArena.Domain.Entities.Equipment;
using KartArena.Domain.Entities.Identity;
using KartArena.Domain.Entities.Payments;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Infrastructure.Database.Seeders;

public static class DynamicDataSeeder
{
    public static async Task SeedAsync(
        DatabaseContext context,
        IPasswordHasher<UserEntity> passwordHasher)
    {
        // Migrations are handled in DatabaseInitializer

        await SeedCitiesAsync(context);
        await SeedPaymentTypesAsync(context);
        await SeedPowertrainTypesAsync(context);
        await SeedRolesAsync(context);
        await SeedUsersAsync(context, passwordHasher);
        await SeedTracksAsync(context);
        await SeedKartsAsync(context);
        await EquipmentSeeder(context);
        await SeedReservationsAsync(context);
        await SeedPaymentsAsync(context);
    }

    private static async Task SeedCitiesAsync(DatabaseContext context)
    {
        if (await context.Cities.AnyAsync())
        {
            Console.WriteLine("ℹ️ Dynamic seed: cities already exist.");
            return;
        }

        var now = DateTime.UtcNow;

        context.Cities.AddRange(
            new CityEntity { Name = "Sarajevo (demo)", PostalCode = "71000 (demo)", Country = "BiH (demo)", CreatedAtUtc = now },
            new CityEntity { Name = "Banja Luka (demo)", PostalCode = "78000 (demo)", Country = "BiH (demo)", CreatedAtUtc = now },
            new CityEntity { Name = "Tuzla (demo)", PostalCode = "75000 (demo)", Country = "BiH (demo)", CreatedAtUtc = now },
            new CityEntity { Name = "Zenica (demo)", PostalCode = "72000 (demo)", Country = "BiH (demo)", CreatedAtUtc = now },
            new CityEntity { Name = "Mostar (demo)", PostalCode = "88000 (demo)", Country = "BiH (demo)", CreatedAtUtc = now },
            new CityEntity { Name = "Bihać (demo)", PostalCode = "77000 (demo)", Country = "BiH (demo)", CreatedAtUtc = now }
        );

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Dynamic seed: cities added.");
    }

    private static async Task SeedPaymentTypesAsync(DatabaseContext context)
    {
        if (await context.PaymentTypes.AnyAsync())
        {
            Console.WriteLine("ℹ️ Dynamic seed: payment types already exist.");
            return;
        }

        var now = DateTime.UtcNow;

        context.PaymentTypes.AddRange(
            new PaymentTypeEntity
            {
                Name = "Online kartično plaćanje",
                Description = "Plaćanje putem online servisa za kartična plaćanja",
                IsDeleted = false,
                isEnabled = true,
                CreatedAtUtc = now,
                ModifiedAtUtc = null
            }
        );

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Dynamic seed: payment types added.");
    }


    private static async Task SeedPowertrainTypesAsync(DatabaseContext context)
    {
        if (await context.PowertrainTypes.AnyAsync())
        {
            Console.WriteLine("ℹ️ Dynamic seed: powertrain types already exist.");
            return;
        }

        var now = DateTime.UtcNow;

        context.PowertrainTypes.AddRange(
            new PowertrainTypeEntity
            {
                Name = "Rotax MAX Evo (demo)",
                Manufacturer = "Rotax (demo)",
                PowerHP = "32 HP (demo)",
                EngineCapacity = "125cc (demo)",
                CreatedAtUtc = now
            },
            new PowertrainTypeEntity
            {
                Name = "Honda GX270 (demo)",
                Manufacturer = "Honda (demo)",
                PowerHP = "9 HP (demo)",
                EngineCapacity = "270cc (demo)",
                CreatedAtUtc = now
            },
            new PowertrainTypeEntity
            {
                Name = "IAME X30 (demo)",
                Manufacturer = "IAME (demo)",
                PowerHP = "30 HP (demo)",
                EngineCapacity = "125cc (demo)",
                CreatedAtUtc = now
            }
        );

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Dynamic seed: powertrain types added.");
    }

    private static async Task SeedRolesAsync(DatabaseContext context)
    {
        if (await context.Roles.AnyAsync())
        {
            Console.WriteLine("ℹ️ Dynamic seed: roles already exist.");
            return;
        }

        var now = DateTime.UtcNow;

        context.Roles.AddRange(
            new RoleEntity { Name = "Admin (demo)", CreatedAtUtc = now },
            new RoleEntity { Name = "User (demo)", CreatedAtUtc = now },
            new RoleEntity { Name = "Employee (demo)", CreatedAtUtc = now }
        );

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Dynamic seed: roles added.");
    }

    private static async Task SeedUsersAsync(
        DatabaseContext context,
        IPasswordHasher<UserEntity> passwordHasher)
    {
        // Only seed if there are no users and no accounts yet.
        if (await context.Users.AnyAsync())
        {
            Console.WriteLine("ℹ️ Dynamic seed: users already exist - skipping.");
            return;
        }

        var now = DateTime.UtcNow;

        // City
        var cityId = await context.Cities
            .Where(c => c.Name == "Sarajevo (demo)")
            .Select(c => c.Id)
            .FirstOrDefaultAsync();

        if (cityId == 0)
            cityId = await context.Cities.Select(c => c.Id).FirstAsync();

        // Roles (adapt name matching if different)
        var adminRoleId = await context.Roles
            .Where(r => r.Name.StartsWith("Admin"))
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var userRoleId = await context.Roles
            .Where(r => r.Name.StartsWith("User"))
            .Select(r => r.Id)
            .FirstOrDefaultAsync();


        var admin = new UserEntity
        {
            Username = "admin",
            Email = "admin@kartarena.local",
            IsEmailConfirmed = true,

            FirstName = "Demo",
            LastName = "Admin",
            DateOfBirth = new DateTime(1995, 1, 1),
            PhoneNumber = null,
            Gender = null,
            Address = null,
            Image = null,

            CityId = cityId,
            RoleId = adminRoleId,

            TotalRaces = 0,
            BestLapTime = null,

            FailedLoginAttempts = 0,
            LastLoginAtUtc = null,

            CreatedAtUtc = now,
            ModifiedAtUtc = null,
            isEnabled = true,
            IsDeleted = false
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin123!");

        var user = new UserEntity
        {
            Username = "user",
            Email = "user@kartarena.local",
            IsEmailConfirmed = true,

            FirstName = "Demo",
            LastName = "User",
            DateOfBirth = new DateTime(1998, 1, 1),
            PhoneNumber = null,
            Gender = null,
            Address = null,
            Image = null,

            CityId = cityId,
            RoleId = userRoleId,

            TotalRaces = 0,
            BestLapTime = null,

            FailedLoginAttempts = 0,
            LastLoginAtUtc = null,

            CreatedAtUtc = now,
            ModifiedAtUtc = null,
            isEnabled = true,
            IsDeleted = false
        };
        user.PasswordHash = passwordHasher.HashPassword(user, "User123!");

        context.Users.AddRange(admin, user);
        await context.SaveChangesAsync(); // get Ids

        Console.WriteLine("✅ Dynamic seed: demo users added.");
    }

    private static async Task EquipmentSeeder(DatabaseContext context)
    {

        if (await context.EquipmentEntity.AnyAsync())
        {
            Console.WriteLine("ℹ️ Dynamic seed: equipment types already exist.");
            return;
        }
      

        var helmetTypeId = 1;
            var suitTypeId = 2;

          context.EquipmentEntity.AddRange(    
              new EquipmentTypeEntity
                {
                    Name = "Helmet",
                    Category = EquipmentCategory.Helmet,
                    Size = "M",
                    Price = 5,
                    Description = "Standard kart helmet",
                    isEnabled = true
                },
                new EquipmentTypeEntity
                {
                    Name = "Racing Suit",
                    Category = EquipmentCategory.Suit,
                    Size = "L",
                    Price = 10,
                    Description = "Kart racing suit",
                    isEnabled = true
                }
            );

        if (await context.EquipmentItemEntity.AnyAsync())
        {
            Console.WriteLine("ℹ️ Dynamic seed: equipment items already exist.");
            return;
        }

        context.EquipmentItemEntity.AddRange(
             new EquipmentItemEntity
             {
                 ItemCode = "HEL-M-001",
                 EquipmentTypeId = helmetTypeId,
                 Status = EquipmentItemStatus.Available
             },
             new EquipmentItemEntity
             {
                 ItemCode = "HEL-M-002",
                 EquipmentTypeId = helmetTypeId,
                 Status = EquipmentItemStatus.Available
             },

             new EquipmentItemEntity
             {
                 ItemCode = "SUI-L-001",
                 EquipmentTypeId = suitTypeId,
                 Status = EquipmentItemStatus.Available
             },
             new EquipmentItemEntity
             {
                 ItemCode = "SUI-L-002",
                 EquipmentTypeId = suitTypeId,
                 Status = EquipmentItemStatus.Available
             },
             new EquipmentItemEntity
             {
                 ItemCode = "SUI-L-003",
                 EquipmentTypeId = suitTypeId,
                 Status = EquipmentItemStatus.Maintenance


             });

        await context.SaveChangesAsync(); // get Ids

        Console.WriteLine("✅ Dynamic seed: demo equipment added.");
    }


    private static async Task SeedReservationsAsync(DatabaseContext context)
{
    if (await context.Reservations.AnyAsync())
    {
        Console.WriteLine("ℹ️ Dynamic seed: reservations already exist.");
        return;
    }

    var users = await context.Users
        .Where(x => !x.IsDeleted)
        .OrderBy(x => x.Id)
        .Take(2)
        .ToListAsync();

    if (users.Count == 0)
    {
        Console.WriteLine("ℹ️ Dynamic seed: no users found - skipping reservations.");
        return;
    }

    var tracks = await context.Tracks
        .Where(x => !x.IsDeleted)
        .OrderBy(x => x.Id)
        .Take(2)
        .ToListAsync();

    if (tracks.Count == 0)
    {
        Console.WriteLine("ℹ️ Dynamic seed: no tracks found - skipping reservations.");
        return;
    }

    var karts = await context.Karts
        .Where(x => !x.IsDeleted)
        .OrderBy(x => x.Id)
        .Take(2)
        .ToListAsync();

    if (karts.Count == 0)
    {
        Console.WriteLine("ℹ️ Dynamic seed: no karts found - skipping reservations.");
        return;
    }

    var today = DateTime.UtcNow.Date;

    var reservations = new List<ReservationEntity>();

    reservations.Add(new ReservationEntity
    {
        UserId = users[0].Id,
        TrackId = tracks[0].Id,
        KartId = karts[0].Id,
        Date = today.AddDays(1),
        StartTime = today.AddDays(1).AddHours(10),
        EndTime = today.AddDays(1).AddHours(10).AddMinutes(30),
        CreatedAtUtc = DateTime.UtcNow,
        ModifiedAtUtc = null,
        IsDeleted = false
    });

    if (users.Count > 1 && tracks.Count > 1 && karts.Count > 1)
    {
        reservations.Add(new ReservationEntity
        {
            UserId = users[1].Id,
            TrackId = tracks[1].Id,
            KartId = karts[1].Id,
            Date = today.AddDays(2),
            StartTime = today.AddDays(2).AddHours(14),
            EndTime = today.AddDays(2).AddHours(14).AddMinutes(45),
            CreatedAtUtc = DateTime.UtcNow,
            ModifiedAtUtc = null,
            IsDeleted = false
        });
    }

    reservations.Add(new ReservationEntity
    {
        UserId = users[0].Id,
        TrackId = tracks[0].Id,
        KartId = karts.Count > 1 ? karts[1].Id : karts[0].Id,
        Date = today.AddDays(3),
        StartTime = today.AddDays(3).AddHours(16),
        EndTime = today.AddDays(3).AddHours(16).AddMinutes(20),
        CreatedAtUtc = DateTime.UtcNow,
        ModifiedAtUtc = null,
        IsDeleted = false
    });

    context.Reservations.AddRange(reservations);
    await context.SaveChangesAsync();

    Console.WriteLine("✅ Dynamic seed: reservations added.");
}

private static async Task SeedPaymentsAsync(DatabaseContext context)
{
    if (await context.Payments.AnyAsync())
    {
        Console.WriteLine("ℹ️ Dynamic seed: payments already exist.");
        return;
    }

    var reservations = await context.Reservations
        .Where(x => !x.IsDeleted)
        .OrderBy(x => x.Id)
        .Take(3)
        .ToListAsync();

    if (reservations.Count == 0)
    {
        Console.WriteLine("ℹ️ Dynamic seed: no reservations found - skipping payments.");
        return;
    }

    var paymentType = await context.PaymentTypes
        .Where(x => !x.IsDeleted)
        .OrderBy(x => x.Id)
        .FirstOrDefaultAsync();

    var now = DateTime.UtcNow;

    var payments = new List<PaymentEntity>();

    payments.Add(new PaymentEntity
    {
        ReservationId = reservations[0].Id,
        PaymentTypeId = paymentType?.Id,
        Amount = 50.00m,
        PaymentDate = now,
        Status = PaymentStatus.Paid,
        TransactionReference = "DEMO-PAY-001",
        Note = "Demo paid reservation",
        CreatedAtUtc = now,
        ModifiedAtUtc = null,
        IsDeleted = false
    });

    if (reservations.Count > 1)
    {
        payments.Add(new PaymentEntity
        {
            ReservationId = reservations[1].Id,
            PaymentTypeId = paymentType?.Id,
            Amount = 65.00m,
            PaymentDate = now.AddMinutes(-30),
            Status = PaymentStatus.Pending,
            TransactionReference = "DEMO-PAY-002",
            Note = "Demo pending reservation",
            CreatedAtUtc = now,
            ModifiedAtUtc = null,
            IsDeleted = false
        });
    }

    if (reservations.Count > 2)
    {
        payments.Add(new PaymentEntity
        {
            ReservationId = reservations[2].Id,
            PaymentTypeId = paymentType?.Id,
            Amount = 40.00m,
            PaymentDate = now.AddHours(-1),
            Status = PaymentStatus.Failed,
            TransactionReference = "DEMO-PAY-003",
            Note = "Demo failed payment",
            CreatedAtUtc = now,
            ModifiedAtUtc = null,
            IsDeleted = false
        });
    }

    context.Payments.AddRange(payments);
    await context.SaveChangesAsync();

    Console.WriteLine("✅ Dynamic seed: payments added.");
}

    private static async Task SeedTracksAsync(DatabaseContext context)
    {
        if (await context.Tracks.AnyAsync())
        {
            Console.WriteLine("ℹ️ Dynamic seed: tracks already exist.");
            return;
        }

        var now = DateTime.UtcNow;

        var cityId = await context.Cities
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        context.Tracks.AddRange(
            new TrackEntity
            {
                Name = "Sarajevo Kart Track (demo)",
                Length = 1200,
                SurfaceType = "Asphalt",
                Outdoors = true,
                CityId = cityId,
                CreatedAtUtc = now
            },
            new TrackEntity
            {
                Name = "Mostar Indoor Track (demo)",
                Length = 800,
                SurfaceType = "Concrete",
                Outdoors = false,
                CityId = cityId,
                CreatedAtUtc = now
            }
        );

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Dynamic seed: tracks added.");
    }

    private static async Task SeedKartsAsync(DatabaseContext context)
    {
        if (await context.Karts.AnyAsync())
        {
            Console.WriteLine("ℹ️ Dynamic seed: karts already exist.");
            return;
        }

        var now = DateTime.UtcNow;

        var powertrainId = await context.PowertrainTypes
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        context.Karts.AddRange(
            new KartEntity
            {
                Name = "Kart 01 (demo)",
                Colour = "Red",
                YearOfManufacture = 2022,
                ChassisNumber = "KART-RED-001",
                Manufacturer = "Sodi Kart",
                PricePerSession = 25,
                Description = "Demo kart for testing reservations",
                PowertrainTypeId = powertrainId,
                CreatedAtUtc = now
            },
            new KartEntity
            {
                Name = "Kart 02 (demo)",
                Colour = "Blue",
                YearOfManufacture = 2023,
                ChassisNumber = "KART-BLUE-002",
                Manufacturer = "CRG",
                PricePerSession = 30,
                Description = "Demo kart for testing reservations",
                PowertrainTypeId = powertrainId,
                CreatedAtUtc = now
            },
            new KartEntity
            {
                Name = "Kart 03 (demo)",
                Colour = "Black",
                YearOfManufacture = 2021,
                ChassisNumber = "KART-BLK-003",
                Manufacturer = "Tony Kart",
                PricePerSession = 28,
                Description = "Demo kart for testing reservations",
                PowertrainTypeId = powertrainId,
                CreatedAtUtc = now
            }
        );

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Dynamic seed: karts added.");
    }
}
