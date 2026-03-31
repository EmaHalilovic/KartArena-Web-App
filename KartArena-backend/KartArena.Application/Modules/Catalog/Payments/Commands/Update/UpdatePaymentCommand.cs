using System.Text.Json.Serialization;
using KartArena.Domain.Entities.Payments;

namespace KartArena.Application.Modules.Catalog.Payments.Commands.Update;

public sealed class UpdatePaymentCommand : IRequest<Unit>
{
    [JsonIgnore]
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public int? PaymentTypeId { get; set; }


    public string? TransactionReference { get; set; }

    public string? Note { get; set; }
}