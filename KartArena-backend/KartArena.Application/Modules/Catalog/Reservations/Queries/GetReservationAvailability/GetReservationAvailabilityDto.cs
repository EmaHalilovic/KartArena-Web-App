namespace KartArena.Application.Modules.Catalog.Reservations.Queries.Availability;

public sealed class GetReservationAvailabilityDto
{
    public List<AvailableTimeDto> AvailableTimes { get; set; } = new();
}

public sealed class AvailableTimeDto
{
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public List<AvailableTrackDto> Tracks { get; set; } = new();
}

public sealed class AvailableTrackDto
{
    public int TrackId { get; set; }
    public string TrackName { get; set; } = string.Empty;
    public int AvailableSlots { get; set; }
    public List<AvailableKartDto> AvailableKarts { get; set; } = new();
}

public sealed class AvailableKartDto
{
    public int KartId { get; set; }
    public string KartName { get; set; } = string.Empty;
    public decimal? PricePerSession { get; set; }
}