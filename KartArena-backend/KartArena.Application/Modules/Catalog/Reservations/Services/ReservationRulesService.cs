using KartArena.Application.Abstractions;
using KartArena.Domain.Entities.Reservations;
using Microsoft.EntityFrameworkCore;

namespace KartArena.Application.Modules.Catalog.Reservations.Services;

public sealed class ReservationRulesService(
    IAppDbContext ctx,
    IBusinessClock businessClock)
    : IReservationRulesService
{
    private const int MaxDriversPerTrackSlot = 6;

    public async Task<ReservationRulesResult> ValidateAsync(
        ReservationRulesRequest request,
        IReadOnlyCollection<ReservationSlot>? pendingSlots,
        CancellationToken ct)
    {
        ValidateDateAndTime(
            request.ReservationDate.Date,
            request.StartTime,
            request.EndTime);

        ValidateDuration(
            request.StartTime,
            request.EndTime);

        var context = await LoadContextAsync(
            [request],
            ct);

        return ValidateAgainstContext(
            request,
            context,
            pendingSlots);
    }

    public async Task<IReadOnlyList<ReservationRulesResult>>ValidateBatchAsync(
            IReadOnlyCollection<ReservationRulesRequest> requests,
            CancellationToken ct)
    {
        if (requests.Count == 0)
        {
            return [];
        }

        /*
         * Validate values that do not require database access before
         * loading the shared validation context.
         */
        foreach (var request in requests)
        {
            ValidateDateAndTime(
                request.ReservationDate.Date,
                request.StartTime,
                request.EndTime);

            ValidateDuration(
                request.StartTime,
                request.EndTime);
        }

        var context = await LoadContextAsync(
            requests,
            ct);

        var results =
            new List<ReservationRulesResult>(
                requests.Count);

        var pendingSlots =
            new List<ReservationSlot>(
                requests.Count);

        /*
         * Results are returned in the same order as the requests.
         */
        foreach (var request in requests)
        {
            var result = ValidateAgainstContext(
                request,
                context,
                pendingSlots);

            results.Add(result);

            pendingSlots.Add(
                new ReservationSlot(
                    request.ReservationDate.Date,
                    request.StartTime,
                    request.EndTime,
                    request.TrackId,
                    request.KartId));
        }

        return results;
    }

    private async Task<ReservationRulesContext> LoadContextAsync(
        IReadOnlyCollection<ReservationRulesRequest> requests,
        CancellationToken ct)
    {
        var trackIds = requests
            .Select(x => x.TrackId)
            .Distinct()
            .ToList();

        var kartIds = requests
            .Select(x => x.KartId)
            .Distinct()
            .ToList();

        var dates = requests
            .Select(x => x.ReservationDate.Date)
            .Distinct()
            .ToList();

        /*
         * Query 1: load all requested enabled tracks.
         */
        var availableTrackIds = await ctx.Tracks
            .AsNoTracking()
            .Where(x =>
                trackIds.Contains(x.Id) &&
                !x.IsDeleted &&
                x.isEnabled)
            .Select(x => x.Id)
            .ToListAsync(ct);

        /*
         * Query 2: load all requested enabled karts and their prices.
         */
        var karts = await ctx.Karts
            .AsNoTracking()
            .Where(x =>
                kartIds.Contains(x.Id) &&
                !x.IsDeleted &&
                x.isEnabled)
            .Select(x => new
            {
                x.Id,
                x.PricePerSession
            })
            .ToListAsync(ct);

        /*
         * Query 3: load reservations that may conflict with any
         * requested track or kart.
         */
        var existingReservations = await ctx.Reservations
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.Status != ReservationStatus.Cancelled &&
                dates.Contains(x.Date.Date) &&
                (
                    trackIds.Contains(x.TrackId) ||
                    kartIds.Contains(x.KartId)
                ))
            .Select(x => new ExistingReservationSlot(
                x.Id,
                x.Date,
                x.StartTime,
                x.EndTime,
                x.TrackId,
                x.KartId))
            .ToListAsync(ct);

        return new ReservationRulesContext(
            availableTrackIds.ToHashSet(),
            karts.ToDictionary(
                x => x.Id,
                x => x.PricePerSession),
            existingReservations);
    }

    private ReservationRulesResult ValidateAgainstContext(
        ReservationRulesRequest request,
        ReservationRulesContext context,
        IReadOnlyCollection<ReservationSlot>? pendingSlots)
    {
        /*
         * Validate again so this method remains safe if it is reused
         * independently in the future.
         */
        ValidateDateAndTime(
            request.ReservationDate.Date,
            request.StartTime,
            request.EndTime);

        ValidateDuration(
            request.StartTime,
            request.EndTime);

        if (!context.AvailableTrackIds.Contains(
                request.TrackId))
        {
            throw new MarketBusinessRuleException(
                "TRACK_NOT_AVAILABLE",
                "Track does not exist or is disabled.");
        }

        if (!context.KartPrices.TryGetValue(
                request.KartId,
                out var kartPrice))
        {
            throw new MarketBusinessRuleException(
                "KART_NOT_AVAILABLE",
                "Kart does not exist or is disabled.");
        }

        if (!kartPrice.HasValue)
        {
            throw new MarketBusinessRuleException(
                "KART_PRICE_MISSING",
                "Selected kart does not have a price.");
        }

        ValidateAvailability(
            request,
            context.ExistingReservations,
            pendingSlots);

        return new ReservationRulesResult(
            kartPrice.Value);
    }

    private void ValidateDateAndTime(
        DateTime date,
        DateTime startTime,
        DateTime endTime)
    {
        if (endTime <= startTime)
        {
            throw new MarketBusinessRuleException(
                "INVALID_RESERVATION_TIME",
                "Reservation end time must be after the start time.");
        }

        if (date != startTime.Date ||
            date != endTime.Date)
        {
            throw new MarketBusinessRuleException(
                "RESERVATION_NOT_SAME_DAY",
                "Reservation date, start time and end time must be on the same day.");
        }

        if (startTime <= businessClock.Now)
        {
            throw new MarketBusinessRuleException(
                "RESERVATION_IN_PAST",
                "Reservation time must be in the future.");
        }
    }

    private static void ValidateDuration(
        DateTime startTime,
        DateTime endTime)
    {
        var durationMinutes =
            (endTime - startTime).TotalMinutes;

        if (durationMinutes is not 10 and not 15)
        {
            throw new MarketBusinessRuleException(
                "INVALID_RESERVATION_DURATION",
                "Only 10-minute and 15-minute sessions are allowed.");
        }
    }

    private static void ValidateAvailability(
        ReservationRulesRequest request,
        IReadOnlyCollection<ExistingReservationSlot> existingReservations,
        IReadOnlyCollection<ReservationSlot>? pendingSlots)
    {
        var date = request.ReservationDate.Date;

        pendingSlots ??= [];

        var overlappingReservations = existingReservations
            .Where(x =>
                x.Date.Date == date &&
                x.StartTime < request.EndTime &&
                x.EndTime > request.StartTime &&
                (
                    !request.ExcludeReservationId.HasValue ||
                    x.ReservationId !=
                    request.ExcludeReservationId.Value
                ))
            .ToList();

        var kartBusyInDatabase =
            overlappingReservations.Any(
                x => x.KartId == request.KartId);

        if (kartBusyInDatabase)
        {
            throw new MarketBusinessRuleException(
                "KART_NOT_AVAILABLE",
                "Selected kart is not available for the chosen time.");
        }

        var kartBusyInPendingSlots =
            pendingSlots.Any(x =>
                x.KartId == request.KartId &&
                x.Date.Date == date &&
                x.StartTime < request.EndTime &&
                x.EndTime > request.StartTime);

        if (kartBusyInPendingSlots)
        {
            throw new MarketBusinessRuleException(
                "KART_NOT_AVAILABLE",
                "Selected kart is already selected for the same time.");
        }

        var existingTrackReservations =
            overlappingReservations.Count(
                x => x.TrackId == request.TrackId);

        var pendingTrackReservations =
            pendingSlots.Count(x =>
                x.TrackId == request.TrackId &&
                x.Date.Date == date &&
                x.StartTime < request.EndTime &&
                x.EndTime > request.StartTime);

        if (existingTrackReservations +
            pendingTrackReservations >=
            MaxDriversPerTrackSlot)
        {
            throw new MarketBusinessRuleException(
                "TRACK_FULL",
                "Track is not available. Maximum 6 drivers are allowed for this time slot.");
        }
    }

    private sealed record ReservationRulesContext(
        IReadOnlySet<int> AvailableTrackIds,
        IReadOnlyDictionary<int, decimal?> KartPrices,
        IReadOnlyCollection<ExistingReservationSlot> ExistingReservations);

    private sealed record ExistingReservationSlot(
        int ReservationId,
        DateTime Date,
        DateTime StartTime,
        DateTime EndTime,
        int TrackId,
        int KartId);
}