namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Update;

public sealed class UpdateReservationCommandHandler(IAppDbContext ctx)
            : IRequestHandler<UpdateReservationCommand, int>
{
    public async Task<int> Handle(UpdateReservationCommand request, CancellationToken ct)
    {
        var reservation = await ctx.Reservations
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, ct);

        if (reservation is null)
            throw new Exception("Reservation not found.");


        if (reservation.StartTime - DateTime.UtcNow < TimeSpan.FromHours(2))
        {
            throw new Exception(
                "Reservation can only be updated at least 2 hours before the start time.");
        }

        // FK checks
        if (!await ctx.Users.AnyAsync(x => x.Id == request.UserId, ct))
            throw new Exception("User does not exist");

        if (!await ctx.Tracks.AnyAsync(x => x.Id == request.TrackId, ct))
            throw new Exception("Track does not exist");

        if (!await ctx.Karts.AnyAsync(x => x.Id == request.KartId, ct))
            throw new Exception("Kart does not exist");

        var newDate = request.ReservationDate.Date;

        // Kart availability (exclude current reservation)
        var kartBusy = await ctx.Reservations.AnyAsync(r =>
            !r.IsDeleted &&
            r.Id != request.Id &&
            r.KartId == request.KartId &&
            r.Date.Date == newDate &&
            r.StartTime < request.EndTime &&
            r.EndTime > request.StartTime,
            ct);

        if (kartBusy)
            throw new Exception("Selected kart is not available for the chosen time slot.");

        // Track capacity (exclude current reservation) max 6
        var trackCount = await ctx.Reservations
            .Where(r =>
                !r.IsDeleted &&
                r.Id != request.Id &&
                r.TrackId == request.TrackId &&
                r.Date.Date == newDate &&
                r.StartTime < request.EndTime &&
                r.EndTime > request.StartTime)
            .CountAsync(ct);

        if (trackCount >= 6)
            throw new Exception("Track is full for the selected time slot.");

        // Update fields
        reservation.UserId = request.UserId;
        reservation.TrackId = request.TrackId;
        reservation.KartId = request.KartId;
        reservation.Date = newDate;
        reservation.StartTime = request.StartTime;
        reservation.EndTime = request.EndTime;

        await ctx.SaveChangesAsync(ct);

        return reservation.Id;
    }
}
