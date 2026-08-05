using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Payments.Queries.GetById
{
    public sealed class GetPaymentReservationDto
    {
        public int Id { get; init; }

        public string CustomerName { get; init; }
            = string.Empty;

        public string CustomerEmail { get; init; }
            = string.Empty;

        public DateTime Date { get; init; }

        public DateTime StartTime { get; init; }

        public DateTime EndTime { get; init; }

        public string TrackName { get; init; }
            = string.Empty;

        public string KartName { get; init; }
            = string.Empty;

        public decimal TotalPrice { get; init; }
    }
}
