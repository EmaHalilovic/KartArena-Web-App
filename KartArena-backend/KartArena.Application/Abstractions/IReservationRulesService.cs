using KartArena.Application.Modules.Catalog.Reservations.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Abstractions
{
    public interface IReservationRulesService
    {
        Task<ReservationRulesResult> ValidateAsync(
            ReservationRulesRequest request,
            IReadOnlyCollection<ReservationSlot>? pendingSlots,
            CancellationToken ct);

        Task<IReadOnlyList<ReservationRulesResult>> ValidateBatchAsync(
          IReadOnlyCollection<ReservationRulesRequest> requests,
          CancellationToken ct);
    }
}
