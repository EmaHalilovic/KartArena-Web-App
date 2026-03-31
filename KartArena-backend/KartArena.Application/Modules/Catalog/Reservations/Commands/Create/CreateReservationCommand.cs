using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Create
{
    public sealed class CreateReservationCommand: IRequest<int>
    {
        public required DateTime ReservationDate { get; set; }
        public required DateTime StartTime { get; set; }
        public required DateTime EndTime { get; set; }
        public required int UserId { get; set; }
        public required int KartId { get; set; }
        public required int TrackId { get; set; }
    }
}
