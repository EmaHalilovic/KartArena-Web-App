using KartArena.Application.Common.Exceptions;
using KartArena.Application.Modules.Catalog.Reservations.Commands.Create;
using KartArena.Application.Modules.Catalog.Reservations.Services;
using KartArena.Domain.Entities.Catalog;
using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;

namespace KartArena.Tests.ReservationTests.UnitTests;

public class ReservationUnitTests
{
    private sealed class TestBusinessClock(DateTime now) : KartArena.Application.Abstractions.IBusinessClock
    {
        public DateTime UtcNow => DateTime.SpecifyKind(now, DateTimeKind.Utc);
        public DateTime Now => now;
        public DateTime Today => now.Date;
        public DateTime ToBusinessTime(DateTime utcDateTime) => utcDateTime;
        public DateTime ToUtc(DateTime businessLocalDateTime) => businessLocalDateTime;
    }

    private DatabaseContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())// Each test gets a new database
            .Options;

        var fakeClock = new Microsoft.Extensions.Time.Testing.FakeTimeProvider();

        return new DatabaseContext(options, fakeClock);
    }

    [Fact]
    public async Task Handle_ShouldAddNewReservation()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();

        var track = new TrackEntity
        {
            Name = "Indoor Track",
            Outdoors = false,
            IsDeleted = false,
            isEnabled=true
        };

        var kart = new KartEntity
        {
            Name = "Kart 1",
            PricePerSession = 15,
            IsDeleted = false,
            isEnabled= true
        };

        var paymentType = new PaymentTypeEntity
        {
            Name = "Stripe Card Payment",
            Code = "STRIPE",
            AllowedOnline = true,
            AllowedAtDesk = false,
            isEnabled = true,
            IsDeleted = false
        };

        context.Tracks.Add(track);
        context.Karts.Add(kart);
        context.PaymentTypes.Add(paymentType);

        await context.SaveChangesAsync();

        var reservationDate = new DateTime(2026, 9, 10);

        var command = new CreateReservationCommand
        {
            ReservationDate = reservationDate,
            StartTime = reservationDate.AddHours(12),
            EndTime = reservationDate.AddHours(12).AddMinutes(10),

            TrackId = track.Id,
            KartId = kart.Id,

            CustomerFirstName = "Test",
            CustomerLastName = "Korisnik",
            CustomerEmail = "test@example.com",
            CustomerPhone = "061111222",

            PaymentTypeId = paymentType.Id
        };

        var businessClock =
            new TestBusinessClock(
                new DateTime(
                    2026,
                    9,
                    1,
                    12,
                    0,
                    0));

        var reservationRulesService =
            new ReservationRulesService(
                context,
                businessClock);

        var handler =
            new CreateReservationCommandHandler(
                context,
                reservationRulesService);

        // Act
        var resultId = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        var reservation = await context.Reservations
            .FindAsync(resultId);

        Assert.NotNull(reservation);
        Assert.Equal("Test", reservation!.CustomerFirstName);
        Assert.Equal("Korisnik", reservation.CustomerLastName);
        Assert.Equal("test@example.com", reservation.CustomerEmail);
        Assert.Equal(track.Id, reservation.TrackId);
        Assert.Equal(kart.Id, reservation.KartId);
        Assert.Equal(15m, reservation.TotalPrice);
        Assert.Equal(ReservationStatus.Pending, reservation.Status);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenKartIsAlreadyReserved()
    {
        // Arrange
        await using var context = GetInMemoryDbContext();

        var track = new TrackEntity
        {
            Name = "Indoor Track",
            Outdoors = false,
            IsDeleted = false,
            isEnabled = true
        };

        var kart = new KartEntity
        {
            Name = "Kart 1",
            PricePerSession = 15,
            IsDeleted = false,
            isEnabled = true
        };

        var paymentType = new PaymentTypeEntity
        {
            Name = "Stripe Card Payment",
            Code = "STRIPE",
            AllowedOnline = true,
            AllowedAtDesk = false,
            isEnabled = true,
            IsDeleted = false
        };

        context.Tracks.Add(track);
        context.Karts.Add(kart);
        context.PaymentTypes.Add(paymentType);

        await context.SaveChangesAsync();

        var reservationDate = new DateTime(2026, 9, 10);
        var startTime = reservationDate.AddHours(12);
        var endTime = startTime.AddMinutes(10);

        var existingReservation = new ReservationEntity
        {
            CustomerFirstName = "Postojeći",
            CustomerLastName = "Korisnik",
            CustomerEmail = "postojeci@example.com",
            CustomerPhone = "061111111",

            TrackId = track.Id,
            KartId = kart.Id,

            Date = reservationDate,
            StartTime = startTime,
            EndTime = endTime,

            Status = ReservationStatus.Confirmed,
            PaymentStatus = PaymentStatus.Pending,
            TotalPrice = 15m,
            IsDeleted = false
        };

        context.Reservations.Add(existingReservation);
        await context.SaveChangesAsync();

        var command = new CreateReservationCommand
        {
            ReservationDate = reservationDate,
            StartTime = startTime,
            EndTime = endTime,

            TrackId = track.Id,
            KartId = kart.Id,

            CustomerFirstName = "Novi",
            CustomerLastName = "Korisnik",
            CustomerEmail = "novi@example.com",
            CustomerPhone = "062222222",

            PaymentTypeId = paymentType.Id
        };

        var businessClock =
          new TestBusinessClock(
              new DateTime(
                  2026,
                  9,
                  1,
                  12,
                  0,
                  0));

        var reservationRulesService =
            new ReservationRulesService(
                context,
                businessClock);

        var handler =
            new CreateReservationCommandHandler(
                context,
                reservationRulesService);

        // Act
        var exception = await Assert.ThrowsAsync<MarketBusinessRuleException>(
            () => handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal(
            "Selected kart is not available for the chosen time.",
            exception.Message);

        Assert.Single(context.Reservations);
    }
}
