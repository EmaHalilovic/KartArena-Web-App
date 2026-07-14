using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Checkout;

public sealed class CheckoutReservationsCommandHandler(IAppDbContext ctx)
    : IRequestHandler<CheckoutReservationsCommand, List<int>>
{
    private const int MaxDriversPerTrackSlot = 6;

    public async Task<List<int>> Handle(CheckoutReservationsCommand request, CancellationToken ct)
    {
        if (request.Items.Count == 0)
            throw new Exception("Cart is empty.");

        ValidateCustomerInfo(request);

        if (request.UserId.HasValue)
        {
            var userExists = await ctx.Users.AnyAsync(x => x.Id == request.UserId.Value, ct);

            if (!userExists)
                throw new Exception("User does not exist.");
        }

        PaymentTypeEntity? paymentType = null;

        if (request.PaymentTypeId.HasValue)
        {
            paymentType = await ctx.PaymentTypes
                .FirstOrDefaultAsync(x => x.Id == request.PaymentTypeId.Value, ct);

            if (paymentType is null)
                throw new Exception("Selected payment type does not exist.");

            if (!paymentType.isEnabled)
                throw new Exception("Selected payment type is not active.");

            if (!paymentType.AllowedOnline)
                throw new Exception("Selected payment type is not available for online payment.");
        }

        var kartIds = request.Items
            .Select(x => x.KartId)
            .Distinct()
            .ToList();

        var selectedKarts = await ctx.Karts
            .Where(k => kartIds.Contains(k.Id))
            .ToListAsync(ct);

        if (selectedKarts.Count != kartIds.Count)
            throw new Exception("One or more selected karts do not exist.");

        var calculatedTotalAmount = request.Items.Sum(item =>
        {
            var kart = selectedKarts.First(k => k.Id == item.KartId);

            if (kart.PricePerSession is null)
                throw new Exception($"Kart {kart.Name ?? kart.Id.ToString()} does not have a price.");

            return kart.PricePerSession.Value;
        });

        if (request.Amount.HasValue && request.Amount.Value != calculatedTotalAmount)
            throw new Exception("Submitted amount does not match calculated amount.");

        await using var transaction = await ctx.Database.BeginTransactionAsync(ct);

        var reservationsToCreate = new List<ReservationEntity>();

        foreach (var item in request.Items)
        {
            var date = item.ReservationDate.Date;

            ValidateReservationItem(item);

            var trackExists = await ctx.Tracks.AnyAsync(x => x.Id == item.TrackId, ct);

            if (!trackExists)
                throw new Exception($"Track with ID {item.TrackId} does not exist.");

            var kart = selectedKarts.First(x => x.Id == item.KartId);

            if (kart.PricePerSession is null)
                throw new Exception($"Kart {kart.Name ?? kart.Id.ToString()} does not have a price.");

            var itemAmount = kart.PricePerSession.Value;

            var kartBusyInDatabase = await ctx.Reservations.AnyAsync(r =>
                    !r.IsDeleted &&
                    r.KartId == item.KartId &&
                    r.Date.Date == date &&
                    r.StartTime < item.EndTime &&
                    r.EndTime > item.StartTime &&
                    r.Status != ReservationStatus.Cancelled,
                ct);

            if (kartBusyInDatabase)
                throw new Exception($"Kart {item.KartId} is not available for the chosen time.");

            var kartBusyInsideCurrentCart = reservationsToCreate.Any(r =>
                r.KartId == item.KartId &&
                r.Date.Date == date &&
                r.StartTime < item.EndTime &&
                r.EndTime > item.StartTime);

            if (kartBusyInsideCurrentCart)
                throw new Exception($"Kart {item.KartId} is already selected in this cart for the same time.");

            var existingTrackReservationsCount = await ctx.Reservations
                .Where(r =>
                    !r.IsDeleted &&
                    r.TrackId == item.TrackId &&
                    r.Date.Date == date &&
                    r.StartTime < item.EndTime &&
                    r.EndTime > item.StartTime &&
                    r.Status != ReservationStatus.Cancelled)
                .CountAsync(ct);

            var cartTrackReservationsCount = reservationsToCreate.Count(r =>
                r.TrackId == item.TrackId &&
                r.Date.Date == date &&
                r.StartTime < item.EndTime &&
                r.EndTime > item.StartTime);

            if (existingTrackReservationsCount + cartTrackReservationsCount >= MaxDriversPerTrackSlot)
                throw new Exception("Track is not available. Maximum 6 drivers are allowed for this time slot.");

            var reservation = new ReservationEntity
            {
                UserId = request.UserId,

                CustomerFirstName = request.CustomerFirstName.Trim(),
                CustomerLastName = request.CustomerLastName.Trim(),
                CustomerEmail = request.CustomerEmail.Trim(),
                CustomerPhone = request.CustomerPhone.Trim(),

                TrackId = item.TrackId,
                KartId = item.KartId,

                Date = date,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                TotalPrice = itemAmount,

                Status = ReservationStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                IsDeleted = false
            };

            if (paymentType is not null)
            {
                reservation.Payment = new PaymentEntity
                {
                    Amount = itemAmount,
                    PaymentTypeId = request.PaymentTypeId,
                    Status = PaymentStatus.Pending,
                    PaymentDate = null,
                    Note = request.PaymentNote,
                    Reservation = reservation
                };
            }

            reservationsToCreate.Add(reservation);
        }

        await ctx.Reservations.AddRangeAsync(reservationsToCreate, ct);
        await ctx.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        return reservationsToCreate.Select(x => x.Id).ToList();
    }

    private static void ValidateCustomerInfo(CheckoutReservationsCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerFirstName))
            throw new Exception("First name is required.");

        if (string.IsNullOrWhiteSpace(request.CustomerLastName))
            throw new Exception("Last name is required.");

        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
            throw new Exception("Email is required.");

        if (string.IsNullOrWhiteSpace(request.CustomerPhone))
            throw new Exception("Phone number is required.");
    }

    private static void ValidateReservationItem(CheckoutReservationItemDto item)
    {
        if (item.TrackId <= 0)
            throw new Exception("Track is required.");

        if (item.KartId <= 0)
            throw new Exception("Kart is required.");

        if (item.StartTime >= item.EndTime)
            throw new Exception("End time must be after start time.");

        if (item.ReservationDate.Date != item.StartTime.Date ||
            item.ReservationDate.Date != item.EndTime.Date)
        {
            throw new Exception("Reservation date, start time and end time must be on the same day.");
        }
    }
}