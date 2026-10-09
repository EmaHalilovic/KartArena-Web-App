using KartArena.Domain.Entities.Reservations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Reservations.Queries.Availability;

public sealed class GetReservationAvailabilityQueryHandler(
    IAppDbContext ctx,
    IBusinessClock businessClock)
    : IRequestHandler<GetReservationAvailabilityQuery, GetReservationAvailabilityDto>
{
    private const int MaxDriversPerTrackSlot = 6;

    public async Task<GetReservationAvailabilityDto> Handle(
        GetReservationAvailabilityQuery request,
        CancellationToken ct)
    {
       ValidateDuration(request.Duration);

        var date = request.Date.Date;

        var tracks = await ctx.Tracks
            .AsNoTracking()
            .Where(x=>x.isEnabled && !x.IsDeleted)
            .ToListAsync(ct);

        var karts = await ctx.Karts
            .AsNoTracking()
            .Where(x=>x.isEnabled && !x.IsDeleted && x.PricePerSession.HasValue)
            .ToListAsync(ct);

        var reservations = await ctx.Reservations
            .AsNoTracking()
            .Where(r =>
                !r.IsDeleted &&
                r.Date.Date == date &&
                r.Status != ReservationStatus.Cancelled)
            .ToListAsync(ct);

        var response = new GetReservationAvailabilityDto();

        foreach (var startTimeString in BuildTimeOptions())
        {
            var startTime = date.Add(ParseTime(startTimeString));
            var endTime = startTime.AddMinutes(request.Duration);

            if (startTime <= businessClock.Now)
                continue;

            var availableTime = new AvailableTimeDto
            {
                StartTime = startTime.ToString("HH:mm"),
                EndTime = endTime.ToString("HH:mm")
            };

            foreach (var track in tracks)
            {
                var overlappingTrackReservations = reservations
                    .Where(r =>
                        r.TrackId == track.Id &&
                        IsOverlapping(
                            r.StartTime,
                            r.EndTime,
                            startTime,
                            endTime))
                    .ToList();

                if (overlappingTrackReservations.Count >= MaxDriversPerTrackSlot)
                    continue;


                var availableKarts = karts
                    .Where(kart => !reservations.Any(r =>
                        r.KartId == kart.Id &&
                        IsOverlapping(
                            r.StartTime,
                            r.EndTime,
                            startTime,
                            endTime)))
                    .Select(kart => new AvailableKartDto
                    {
                        KartId = kart.Id,
                        KartName = string.IsNullOrWhiteSpace(kart.Name)
                            ? $"Kart #{kart.Id}"
                            : kart.Name,
                        PricePerSession = kart.PricePerSession
                    })
                    .ToList();

                if (availableKarts.Count == 0)
                    continue;

                availableTime.Tracks.Add(new AvailableTrackDto
                {
                    TrackId = track.Id,
                    TrackName = string.IsNullOrWhiteSpace(track.Name)
                        ? $"Track #{track.Id}"
                        : track.Name,
                    AvailableSlots = MaxDriversPerTrackSlot - overlappingTrackReservations.Count,
                    AvailableKarts = availableKarts
                });
            }

            if (availableTime.Tracks.Count > 0)
            {
                response.AvailableTimes.Add(availableTime);
            }
        }

        return response;
    }

    private static void ValidateDuration(int duration)
    {
        if (duration is not 10 and not 15)
        {
            throw new MarketBusinessRuleException(
                "INVALID_RESERVATION_DURATION",
                "Only 10-minute and 15-minute sessions are allowed.");
        }
    }

    private static bool IsOverlapping(
        DateTime existingStart,
        DateTime existingEnd,
        DateTime requestedStart,
        DateTime requestedEnd)
    {
        return existingStart < requestedEnd &&
               existingEnd > requestedStart;
    }

    private static List<string> BuildTimeOptions()
    {
        var options = new List<string>();

        for (var hour = 10; hour <= 22; hour++)
        {
            foreach (var minute in new[] { 0, 15, 30, 45 })
            {
                if (hour == 22 && minute > 0)
                {
                    continue;
                }

                options.Add(
                    $"{hour:00}:{minute:00}");
            }
        }

        return options;
    }

    private static TimeSpan ParseTime(string time)
    {
        var parts = time.Split(':');

        var hours =
            int.Parse(parts[0]);

        var minutes =
            int.Parse(parts[1]);

        return new TimeSpan(
            hours,
            minutes,
            0);
    }
}
