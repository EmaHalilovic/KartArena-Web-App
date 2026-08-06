namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Update;

public sealed class UpdateReservationCommand : IRequest<int>
{
    [JsonIgnore]
    public int Id { get; set; }

    public int? UserId { get; set; }
    public int TrackId { get; set; }
    public int KartId { get; set; }

    public DateTime ReservationDate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
