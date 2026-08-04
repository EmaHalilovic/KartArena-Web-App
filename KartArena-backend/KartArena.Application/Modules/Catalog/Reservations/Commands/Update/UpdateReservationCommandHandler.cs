using KartArena.Domain.Entities.Payments;
using KartArena.Domain.Entities.Reservations;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Update;

public sealed class UpdateReservationCommandHandler(IAppDbContext ctx)
    : IRequestHandler<UpdateReservationCommand, int>
{
    public async Task<int> Handle(UpdateReservationCommand request, CancellationToken ct)
    {
        var reservation = await ctx.Reservations
            .Include(r => r.Payment)
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, ct);

        if (reservation is null)
            throw new Exception("Reservation not found.");

        // Only confirmed reservations may be edited.
        if (reservation.Status != ReservationStatus.Confirmed)
        {
            throw new Exception("Only confirmed reservations can be updated.");
        }

        // Do not allow update after payment is completed
        if (reservation.PaymentStatus == PaymentStatus.Paid)
        {
            throw new Exception("Paid reservations cannot be updated.");
        }

        // Current reservation start must be at least 2 hours in the future
        var currentReservationStartUtc =
            reservation.Date.Date.Add(reservation.StartTime.TimeOfDay);

        if (currentReservationStartUtc - DateTime.UtcNow < TimeSpan.FromHours(2))
        {
            throw new Exception("Reservation can only be updated at least 2 hours before the start time.");
        }

        // Foreign key checks
        if (request.UserId.HasValue &&
            !await ctx.Users.AnyAsync(x => x.Id == request.UserId.Value, ct))
            throw new Exception("User does not exist.");

        if (!await ctx.Tracks.AnyAsync(x => x.Id == request.TrackId, ct))
            throw new Exception("Track does not exist.");

        if (!await ctx.Karts.AnyAsync(x => x.Id == request.KartId, ct))
            throw new Exception("Kart does not exist.");

        var newDate = request.ReservationDate.Date;
        var newStart = request.StartTime;
        var newEnd = request.EndTime;

        // Optional: also block updates to new times that are too close
        var newReservationStartUtc = newDate.Add(newStart.TimeOfDay);
        if (newReservationStartUtc - DateTime.UtcNow < TimeSpan.FromHours(2))
        {
            throw new Exception("Updated reservation must start at least 2 hours from now.");
        }

        // Kart availability - exclude current reservation
        var kartBusy = await ctx.Reservations.AnyAsync(r =>
            !r.IsDeleted &&
            r.Id != request.Id &&
            r.KartId == request.KartId &&
            r.Date.Date == newDate &&
            r.StartTime < newEnd &&
            r.EndTime > newStart,
            ct);

        if (kartBusy)
            throw new Exception("Selected kart is not available for the chosen time slot.");

        // Track capacity - exclude current reservation
        var trackCount = await ctx.Reservations
            .Where(r =>
                !r.IsDeleted &&
                r.Id != request.Id &&
                r.TrackId == request.TrackId &&
                r.Date.Date == newDate &&
                r.StartTime < newEnd &&
                r.EndTime > newStart)
            .CountAsync(ct);

        if (trackCount >= 6)
            throw new Exception("Track is full for the selected time slot.");

        reservation.UserId = request.UserId;
        reservation.TrackId = request.TrackId;
        reservation.KartId = request.KartId;
        reservation.Date = newDate;
        reservation.StartTime = newStart;
        reservation.EndTime = newEnd;

        await ctx.SaveChangesAsync(ct);

        return reservation.Id;
    }
}
