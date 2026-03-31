namespace KartArena.Application.Modules.Catalog.Reservations.Queries.List;

public sealed class ListReservationsQueryDto
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int TrackId { get; set; }
    public int KartId { get; set; }

    public DateTime Date { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public string? UserFirstName { get; set; }
    public string? UserLastName { get; set; }

    public string? TrackName { get; set; }
    public string? KartName { get; set; }
}
