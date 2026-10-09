using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Reservations.Services
{
    public sealed record ReservationRulesRequest(
      DateTime ReservationDate,
      DateTime StartTime,
      DateTime EndTime,
      int TrackId,
      int KartId,
      int? ExcludeReservationId = null);
}
