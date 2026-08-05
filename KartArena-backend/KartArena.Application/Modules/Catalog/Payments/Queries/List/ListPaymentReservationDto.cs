using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Payments.Queries.List
{
    public sealed class ListPaymentReservationDto
    {
        public int Id { get; init; }

        public DateTime Date { get; init; }

        public DateTime StartTime { get; init; }

        public DateTime EndTime { get; init; }

        public string TrackName { get; init; } = string.Empty;

        public string KartName { get; init; } = string.Empty;
    }
}
