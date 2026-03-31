using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Reservations.Commands.Delete;
public sealed record DeleteReservationCommand(int ReservationId) : IRequest;