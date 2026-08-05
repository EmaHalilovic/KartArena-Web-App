using KartArena.Domain.Entities.Equipment;
using KartArena.Domain.Entities.Identity;
using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
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
        await SeedPaymentsAsync(context);
        await SeedReservationsAsync(context);
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
                Name = "Stripe Card Payment",
                Code = "STRIPE",
                AllowedOnline = true,
                AllowedAtDesk = false,
                Description = "Online card payment through Stripe.",
                IsDeleted = false,
                isEnabled = true,
                CreatedAtUtc = now,
                ModifiedAtUtc = null
            },
            new PaymentTypeEntity
            {
                Name = "Cash payment",
                Code = "DESK_CASH",
                Description = "Payment made in cash at the front desk",
                AllowedOnline = false,
                AllowedAtDesk = true,
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

        // Roles
        var adminRoleId = await context.Roles
            .Where(r => r.Name.StartsWith("Admin"))
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var userRoleId = await context.Roles
            .Where(r => r.Name.StartsWith("User"))
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var employeeRoleId = await context.Roles
            .Where(r => r.Name.StartsWith("Employee"))
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        var users = new List<UserEntity>();

        // ADMIN
        users.Add(new UserEntity
        {
            Username = "admin",
            Email = "admin@kartarena.local",
            IsEmailConfirmed = true,
            FirstName = "Demo",
            LastName = "Admin",
            DateOfBirth = new DateTime(1995, 1, 1),
            CityId = cityId,
            RoleId = adminRoleId,
            CreatedAtUtc = now,
            isEnabled = true,
            IsDeleted = false
        });

        // REGULAR USERS
        users.AddRange(new[]
        {
        new UserEntity
        {
            Username = "user",
            Email = "user@kartarena.local",
            FirstName = "Demo",
            LastName = "User",
            DateOfBirth = new DateTime(1998, 1, 1),
            CityId = cityId,
            RoleId = userRoleId,
            IsEmailConfirmed = true,
            CreatedAtUtc = now,
            isEnabled = true
        },
        new UserEntity
        {
            Username = "john.doe",
            Email = "john.doe@kartarena.local",
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1992, 5, 10),
            CityId = cityId,
            RoleId = userRoleId,
            IsEmailConfirmed = true,
            CreatedAtUtc = now,
            isEnabled = true
        },
        new UserEntity
        {
            Username = "jane.smith",
            Email = "jane.smith@kartarena.local",
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = new DateTime(1996, 3, 22),
            CityId = cityId,
            RoleId = userRoleId,
            IsEmailConfirmed = true,
            CreatedAtUtc = now,
            isEnabled = true
        },
        new UserEntity
        {
            Username = "alex",
            Email = "alex@kartarena.local",
            FirstName = "Alex",
            LastName = "Taylor",
            DateOfBirth = new DateTime(2000, 7, 15),
            CityId = cityId,
            RoleId = userRoleId,
            IsEmailConfirmed = true,
            CreatedAtUtc = now,
            isEnabled = true
        }
    });

        // EMPLOYEES
        users.AddRange(new[]
        {
        new UserEntity
        {
            Username = "employee1",
            Email = "employee1@kartarena.local",
            FirstName = "Mark",
            LastName = "Johnson",
            DateOfBirth = new DateTime(1990, 8, 12),
            CityId = cityId,
            RoleId = employeeRoleId,
            IsEmailConfirmed = true,
            CreatedAtUtc = now,
            isEnabled = true
        },
        new UserEntity
        {
            Username = "employee2",
            Email = "employee2@kartarena.local",
            FirstName = "Sara",
            LastName = "Williams",
            DateOfBirth = new DateTime(1993, 11, 5),
            CityId = cityId,
            RoleId = employeeRoleId,
            IsEmailConfirmed = true,
            CreatedAtUtc = now,
            isEnabled = true
        },
        new UserEntity
        {
            Username = "employee3",
            Email = "employee3@kartarena.local",
            FirstName = "David",
            LastName = "Brown",
            DateOfBirth = new DateTime(1989, 2, 18),
            CityId = cityId,
            RoleId = employeeRoleId,
            IsEmailConfirmed = true,
            CreatedAtUtc = now,
            isEnabled = true
        }
    });

        // HASH PASSWORDS
        foreach (var u in users)
        {
            u.PasswordHash = passwordHasher.HashPassword(
                u,
                u.RoleId == adminRoleId ? "Admin123!" :
                u.RoleId == employeeRoleId ? "Employee123!" :
                "User123!"
            );

            u.TotalRaces = 0;
            u.BestLapTime = null;
            u.FailedLoginAttempts = 0;
            u.LastLoginAtUtc = null;
            u.ModifiedAtUtc = null;
            u.IsDeleted = false;
        }

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Dynamic seed: users & employees added.");
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

    private static async Task SeedReservationsAsync(
    DatabaseContext context)
    {
        if (await context.Reservations.AnyAsync())
        {
            Console.WriteLine(
                "ℹ️ Dynamic seed: reservations already exist.");

            return;
        }

        var userRoleId = await context.Roles
            .Where(role => role.Name.StartsWith("User"))
            .Select(role => role.Id)
            .FirstAsync();

        var users = await context.Users
            .Where(user =>
                !user.IsDeleted &&
                user.RoleId == userRoleId)
            .OrderBy(user => user.Id)
            .Take(3)
            .ToListAsync();

        var tracks = await context.Tracks
            .Where(track => !track.IsDeleted)
            .OrderBy(track => track.Id)
            .ToListAsync();

        var karts = await context.Karts
            .Where(kart => !kart.IsDeleted)
            .OrderBy(kart => kart.Id)
            .ToListAsync();

        if (users.Count < 3 ||
            tracks.Count < 2 ||
            karts.Count < 2)
        {
            Console.WriteLine(
                "ℹ️ Dynamic seed: not enough users, tracks, or karts.");

            return;
        }

        var now = DateTime.UtcNow;
        var today = now.Date;

        var reservations = new List<ReservationEntity>
    {
        new()
        {
            UserId = users[0].Id,

            CustomerFirstName = users[0].FirstName,
            CustomerLastName = users[0].LastName,
            CustomerEmail = users[0].Email ?? "user1@demo.com",
            CustomerPhone = "061111111",

            TrackId = tracks[0].Id,
            KartId = karts[0].Id,

            Date = today.AddDays(-2),
            StartTime = today.AddDays(-2).AddHours(18),
            EndTime = today.AddDays(-2).AddHours(18).AddMinutes(30),

            TotalPrice = 40m,

            Status = ReservationStatus.Completed,
            PaymentStatus = PaymentStatus.Paid,

            CreatedAtUtc = now.AddDays(-3),
            IsDeleted = false
        },

        new()
        {
            UserId = users[1].Id,

            CustomerFirstName = users[1].FirstName,
            CustomerLastName = users[1].LastName,
            CustomerEmail = users[1].Email ?? "user2@demo.com",
            CustomerPhone = "062222222",

            TrackId = tracks[1].Id,
            KartId = karts[1].Id,

            Date = today.AddDays(-1),
            StartTime = today.AddDays(-1).AddHours(14),
            EndTime = today.AddDays(-1).AddHours(14).AddMinutes(45),

            TotalPrice = 55m,

            Status = ReservationStatus.Completed,
            PaymentStatus = PaymentStatus.Paid,

            CreatedAtUtc = now.AddDays(-2),
            IsDeleted = false
        },

        new()
        {
            UserId = users[2].Id,

            CustomerFirstName = users[2].FirstName,
            CustomerLastName = users[2].LastName,
            CustomerEmail = users[2].Email ?? "user3@demo.com",
            CustomerPhone = "063333333",

            TrackId = tracks[0].Id,
            KartId = karts[1].Id,

            Date = today.AddDays(-1),
            StartTime = today.AddDays(-1).AddHours(20),
            EndTime = today.AddDays(-1).AddHours(20).AddMinutes(30),

            TotalPrice = 50m,

            Status = ReservationStatus.Cancelled,
            PaymentStatus = PaymentStatus.Failed,

            CreatedAtUtc = now.AddDays(-2),
            IsDeleted = false
        },

        new()
        {
            UserId = users[0].Id,

            CustomerFirstName = users[0].FirstName,
            CustomerLastName = users[0].LastName,
            CustomerEmail = users[0].Email ?? "user1@demo.com",
            CustomerPhone = "061111111",

            TrackId = tracks[1].Id,
            KartId = karts[0].Id,

            Date = today.AddDays(1),
            StartTime = today.AddDays(1).AddHours(16),
            EndTime = today.AddDays(1).AddHours(16).AddMinutes(20),

            TotalPrice = 30m,

            Status = ReservationStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,

            CreatedAtUtc = now,
            IsDeleted = false
        },

        new()
        {
            UserId = users[1].Id,

            CustomerFirstName = users[1].FirstName,
            CustomerLastName = users[1].LastName,
            CustomerEmail = users[1].Email ?? "user2@demo.com",
            CustomerPhone = "062222222",

            TrackId = tracks[0].Id,
            KartId = karts[0].Id,

            Date = today.AddDays(2),
            StartTime = today.AddDays(2).AddHours(19),
            EndTime = today.AddDays(2).AddHours(19).AddMinutes(40),

            TotalPrice = 45m,

            Status = ReservationStatus.Confirmed,
            PaymentStatus = PaymentStatus.Pending,

            CreatedAtUtc = now,
            IsDeleted = false
        },

        new()
        {
            UserId = users[1].Id,

            CustomerFirstName = users[1].FirstName,
            CustomerLastName = users[1].LastName,
            CustomerEmail = users[1].Email ?? "user2@demo.com",
            CustomerPhone = "062222222",

            TrackId = tracks[0].Id,

            // Use a different kart to avoid overlapping the previous reservation.
            KartId = karts[1].Id,

            Date = today.AddDays(2),
            StartTime = today.AddDays(2).AddHours(19),
            EndTime = today.AddDays(2).AddHours(19).AddMinutes(40),

            TotalPrice = 45m,

            Status = ReservationStatus.Confirmed,
            PaymentStatus = PaymentStatus.Pending,

            CreatedAtUtc = now,
            IsDeleted = false
        }
    };

        await context.Reservations.AddRangeAsync(reservations);
        await context.SaveChangesAsync();

        Console.WriteLine(
            "✅ Dynamic seed: reservations added.");
    }
    private static async Task SeedPaymentsAsync(
    DatabaseContext context)
    {
        if (await context.Payments.AnyAsync())
        {
            Console.WriteLine(
                "ℹ️ Dynamic seed: payments already exist.");

            return;
        }

        var reservations = await context.Reservations
            .Where(reservation => !reservation.IsDeleted)
            .OrderBy(reservation => reservation.Id)
            .Take(6)
            .ToListAsync();

        if (reservations.Count == 0)
        {
            Console.WriteLine(
                "ℹ️ Dynamic seed: no reservations found - skipping payments.");

            return;
        }

        var card = await context.PaymentTypes
            .FirstOrDefaultAsync(paymentType =>
                !paymentType.IsDeleted &&
                paymentType.Code == "STRIPE");

        var cash = await context.PaymentTypes
            .FirstOrDefaultAsync(paymentType =>
                !paymentType.IsDeleted &&
                paymentType.Code == "DESK_CASH");

        if (card is null || cash is null)
        {
            Console.WriteLine(
                "ℹ️ Dynamic seed: required payment types were not found.");

            return;
        }

        var now = DateTime.UtcNow;

        var payments = new List<PaymentEntity>();
        var links = new List<PaymentReservationEntity>();

        if (reservations.Count >= 1)
        {
            var payment = new PaymentEntity
            {
                PaymentTypeId = card.Id,

                Amount = reservations[0].TotalPrice,
                Currency = "bam",

                PaymentDate = now.AddDays(-2),
                Status = PaymentStatus.Paid,

                TransactionReference = "TX-CARD-001",
                Note = "Demo Stripe card payment",

                CreatedAtUtc = now.AddDays(-2),
                ModifiedAtUtc = null,

                IsDeleted = false,
                isEnabled = true
            };

            payments.Add(payment);

            links.Add(new PaymentReservationEntity
            {
                Payment = payment,
                Reservation = reservations[0]
            });
        }

        if (reservations.Count >= 2)
        {
            var payment = new PaymentEntity
            {
                PaymentTypeId = cash.Id,

                Amount = reservations[1].TotalPrice,
                Currency = "bam",

                PaymentDate = now.AddDays(-1),
                Status = PaymentStatus.Paid,

                TransactionReference = "TX-CASH-002",
                Note = "Paid at desk",

                CreatedAtUtc = now.AddDays(-1),
                ModifiedAtUtc = null,

                IsDeleted = false,
                isEnabled = true
            };

            payments.Add(payment);

            links.Add(new PaymentReservationEntity
            {
                Payment = payment,
                Reservation = reservations[1]
            });
        }

        if (reservations.Count >= 3)
        {
            var payment = new PaymentEntity
            {
                PaymentTypeId = card.Id,

                Amount = reservations[2].TotalPrice,
                Currency = "bam",

                PaymentDate = null,
                Status = PaymentStatus.Failed,

                TransactionReference = "TX-CARD-003",
                Note = "User cancelled before payment",

                CreatedAtUtc = now.AddDays(-1),
                ModifiedAtUtc = null,

                IsDeleted = false,
                isEnabled = true
            };

            payments.Add(payment);

            links.Add(new PaymentReservationEntity
            {
                Payment = payment,
                Reservation = reservations[2]
            });
        }

        if (reservations.Count >= 4)
        {
            var payment = new PaymentEntity
            {
                PaymentTypeId = card.Id,

                Amount = reservations[3].TotalPrice,
                Currency = "bam",

                PaymentDate = null,
                Status = PaymentStatus.Pending,

                Note = "Awaiting online payment",

                CreatedAtUtc = now,
                ModifiedAtUtc = null,

                IsDeleted = false,
                isEnabled = true
            };

            payments.Add(payment);

            links.Add(new PaymentReservationEntity
            {
                Payment = payment,
                Reservation = reservations[3]
            });
        }

        if (reservations.Count >= 6)
        {
            // This payment demonstrates one transaction covering
            // multiple reservations from the same cart.
            var groupedReservations = reservations
                .Skip(4)
                .Take(2)
                .ToList();

            var groupedPayment = new PaymentEntity
            {
                PaymentTypeId = cash.Id,

                Amount = groupedReservations.Sum(
                    reservation => reservation.TotalPrice),

                Currency = "bam",

                PaymentDate = null,
                Status = PaymentStatus.Pending,

                Note = "Desk payment for multiple reservations",

                CreatedAtUtc = now,
                ModifiedAtUtc = null,

                IsDeleted = false,
                isEnabled = true
            };

            payments.Add(groupedPayment);

            foreach (var reservation in groupedReservations)
            {
                links.Add(new PaymentReservationEntity
                {
                    Payment = groupedPayment,
                    Reservation = reservation
                });
            }
        }

        await context.Payments.AddRangeAsync(payments);
        await context.PaymentReservations.AddRangeAsync(links);

        await context.SaveChangesAsync();

        Console.WriteLine(
            "✅ Dynamic seed: payments and payment-reservation links added.");
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
