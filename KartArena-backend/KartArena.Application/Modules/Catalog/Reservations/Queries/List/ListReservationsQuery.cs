namespace KartArena.Application.Modules.Catalog.Reservations.Queries.List
{
    public sealed class ListReservationsQuery : BasePagedQuery<ListReservationsQueryDto>
    {
        public string? Search { get; init; }
        public int? UserId { get; init; }
        public int? TrackId { get; init; }
        public int? KartId { get; init; }

        public Domain.Entities.Reservations.ReservationStatus? Status { get; init; }
        public Domain.Entities.Payments.PaymentStatus? PaymentStatus { get; init; }

        public DateTime? DateFrom { get; init; }
        public DateTime? DateTo { get; init; }
    }
}