using KartArena.Domain.Entities.Payments;

namespace KartArena.Application.Modules.Catalog.Payments.Queries.List;

public sealed class ListPaymentsQuery : BasePagedQuery<ListPaymentsQueryDto>
{
    public string? Search { get; set; }

    public int? ReservationId { get; set; }

    public int? PaymentTypeId { get; set; }

    public PaymentStatus? Status { get; set; }

}