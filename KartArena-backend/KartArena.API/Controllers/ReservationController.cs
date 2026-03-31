
using KartArena.Application.Modules.Catalog.Equipment.Queries.GetById;
using KartArena.Application.Modules.Catalog.Reservations.Commands.Create;
using KartArena.Application.Modules.Catalog.Reservations.Commands.Delete;
using KartArena.Application.Modules.Catalog.Reservations.Commands.Update;
using KartArena.Application.Modules.Catalog.Reservations.Queries.GetById;
using KartArena.Application.Modules.Catalog.Reservations.Queries.List;

namespace KartArena.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("reservation/controller")]
    public class ReservationController(ISender sender) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<int>> CreateReservation(CreateReservationCommand command, CancellationToken ct)
        {
            if (command == null)
                return BadRequest("Invalid request body.");
            int id = await sender.Send(command, ct);
            return Created("", new { id });
        }
        [HttpPut("{id:int}")]
        public async Task UpdateReservation(int id, UpdateReservationCommand command, CancellationToken ct)
        {
            command.Id = id;
            await sender.Send(command, ct);
        }
        [HttpDelete("{id:int}")]
        public async Task DeleteReservation(int id, CancellationToken ct)
        {
            await sender.Send(new DeleteReservationCommand(id), ct);
        }
        [HttpGet("{id:int}")]
        public async Task<GetReservationByIdQueryDto> GetById(int id, CancellationToken ct)
        {
            var reservation = await sender.Send(new GetReservationByIdQuery(id), ct);
            return reservation; // if NotFoundException -> 404 via middleware
        }
        [HttpGet]
        public async Task<PageResult<ListReservationsQueryDto>> List([FromQuery] ListReservationsQuery query, CancellationToken ct)
        {
            var result = await sender.Send(query, ct);
            return result; // if NotFoundException -> 404 via middleware
        }

    }
}
