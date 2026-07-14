using KartArena.Application.Abstractions;
using KartArena.Application.Modules.Catalog.Payments.Commands.Create;
using KartArena.Application.Modules.Catalog.Payments.Commands.Delete;
using KartArena.Application.Modules.Catalog.Payments.Commands.Update;
using KartArena.Application.Modules.Catalog.Payments.Queries.GetById;
using KartArena.Application.Modules.Catalog.Payments.Queries.List;
using KartArena.Application.Modules.Catalog.Payments.Stripe.Models;

namespace KartArena.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("payments/controller")]
    public class PaymentsController(
        ISender sender,
        IStripePaymentService stripePaymentService)
        : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<int>> CreatePayment(
            CreatePaymentCommand command,
            CancellationToken ct)
        {
            if (command == null)
                return BadRequest("Invalid request body.");

            int id = await sender.Send(command, ct);

            return Created(
                $"/payments/controller/{id}",
                new { id });
        }

        [HttpGet("{id:int}")]
        public async Task<GetPaymentByIdQueryDto> GetById(
            int id,
            CancellationToken ct)
        {
            return await sender.Send(
                new GetPaymentByIdQuery(id),
                ct);
        }

        [HttpGet]
        public async Task<PageResult<ListPaymentsQueryDto>> List(
            [FromQuery] ListPaymentsQuery query,
            CancellationToken ct)
        {
            return await sender.Send(query, ct);
        }

        [HttpDelete("{id:int}")]
        public async Task Delete(
            int id,
            CancellationToken cancellationToken)
        {
            await sender.Send(
                new DeletePaymentCommand { Id = id },
                cancellationToken);
        }

        [HttpPut("{id:int}")]
        public async Task Update(
            int id,
            UpdatePaymentCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;

            await sender.Send(
                command,
                cancellationToken);
        }

        [HttpPost("checkout-session")]
        public async Task<ActionResult<CreateCheckoutSessionResponse>>
            CreateCheckoutSession(
                [FromBody] CreateCheckoutSessionRequest request,
                CancellationToken cancellationToken)
        {
            try
            {
                var response =
                    await stripePaymentService
                        .CreateCheckoutSessionAsync(
                            request.ReservationId,
                            cancellationToken);

                return Ok(response);
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new
                {
                    message = exception.Message
                });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
        }
    }
}