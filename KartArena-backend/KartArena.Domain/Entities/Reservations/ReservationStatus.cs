using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Reservations
{
    public enum ReservationStatus
    {
        Pending = 1,
        Confirmed = 2,
        Completed = 3,
        Cancelled = 4
    }
}
