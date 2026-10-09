using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Reservations.Services
{
    public sealed record ReservationRulesResult(
        decimal TotalPrice);
}
