using KartArena.Application.Features.ReservationEmployeeAssignments.Commands.AssignReservationResources;
using KartArena.Application.Features.ReservationEmployeeAssignments.Commands.RemoveReservationEmployeeAssignment;
using KartArena.Application.Features.ReservationEmployeeAssignments.DTOs;
using KartArena.Application.Features.ReservationEmployeeAssignments.Queries.GetAvailableEquipmentForReservation;
using KartArena.Application.Features.ReservationEmployeeAssignments.Queries.GetReservationAssignmentsByReservationId;
using KartArena.Application.Modules.Catalog.Reservations.Commands.Checkout;
using KartArena.Application.Modules.Catalog.Reservations.Commands.Create;
using KartArena.Application.Modules.Catalog.Reservations.Commands.Delete;
using KartArena.Application.Modules.Catalog.Reservations.Commands.MarkCashPaymentAsPaid;
using KartArena.Application.Modules.Catalog.Reservations.Commands.Update;
using KartArena.Application.Modules.Catalog.Reservations.Queries.Availability;
using KartArena.Application.Modules.Catalog.Reservations.Queries.GetById;
using KartArena.Application.Modules.Catalog.Reservations.Queries.List;

namespace KartArena.Api.Controllers.Catalog
{
    [AllowAnonymous]
    [Route("reservations")]
    [Route("api/reservations")]
    [ApiController]
    public sealed class ReservationsController(ISender sender) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<int>> Create([FromBody] CreateReservationCommand command, CancellationToken cancellationToken)
        {
            var id = await sender.Send(command, cancellationToken);
            return Ok(id);
        }

        [HttpPost("checkout")]
        [AllowAnonymous]
        public async Task<ActionResult<List<int>>> Checkout(CheckoutReservationsCommand command,CancellationToken ct)
        {
            var reservationIds = await sender.Send(command, ct);

            return Ok(reservationIds);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<int>> Update(
            int id,
            [FromBody] UpdateReservationCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await sender.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            await sender.Send(new DeleteReservationCommand(id), cancellationToken);
            return NoContent();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetReservationByIdQueryDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetReservationByIdQuery(id), cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<PageResult<ListReservationsQueryDto>>> List(
            [FromQuery] ListReservationsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(query, cancellationToken);
            return Ok(result);
        }
        
        [HttpGet("availability")]
        [AllowAnonymous]
        public async Task<ActionResult<GetReservationAvailabilityDto>> GetAvailability( [FromQuery] DateTime date,[FromQuery] int duration, CancellationToken ct)
        {
            var result = await sender.Send(new GetReservationAvailabilityQuery
            {
                Date = date,
                Duration = duration
            }, ct);

            return Ok(result);
        }


        [HttpPut("{reservationId:int}/pay-cash")]
        [AllowAnonymous]
        public async Task<ActionResult<int>> MarkCashPaymentAsPaid(
            int reservationId,
            [FromBody] MarkCashReservationPaymentAsPaidCommand command,
            CancellationToken cancellationToken)
        {
            command.ReservationId = reservationId;
            var result = await sender.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{reservationId:int}/assignments")]
        public async Task<ActionResult<List<ReservationEmployeeAssignmentDto>>> GetAssignments(
            int reservationId,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new GetReservationAssignmentsByReservationIdQuery { ReservationId = reservationId },
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("{reservationId:int}/available-equipment")]
        public async Task<ActionResult<List<AvailableEquipmentItemDto>>> GetAvailableEquipment(
            int reservationId,
            [FromQuery] int categoryId,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new GetAvailableEquipmentForReservationQuery
                {
                    ReservationId = reservationId,
                    EquipmentTypeId = categoryId
                },
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("{reservationId:int}/assignments")]
        public async Task<ActionResult<int>> SaveAssignments(
            int reservationId,
            [FromBody] AssignReservationResourcesCommand command,
            CancellationToken cancellationToken)
        {
            command.ReservationId = reservationId;
            var result = await sender.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("assignments/{assignmentId:int}")]
        public async Task<IActionResult> DeleteAssignment(
            int assignmentId,
            CancellationToken cancellationToken)
        {
            await sender.Send(
                new RemoveReservationEmployeeAssignmentCommand { Id = assignmentId },
                cancellationToken);

            return NoContent();
        }
    }
}
