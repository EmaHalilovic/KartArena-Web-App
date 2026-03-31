using KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Create;
using KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Delete;
using KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Status.Disable;
using KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Status.Enable;
using KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Update;
using KartArena.Application.Modules.Catalog.PaymentTypes.Queries.GetById;
using KartArena.Application.Modules.Catalog.PaymentTypes.Queries.List;

namespace KartArena.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("payment-types/controller")]
public sealed class PaymentTypesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreatePaymentTypeCommand command, CancellationToken ct)
    {
        if (command == null)
            return BadRequest("Invalid request body.");

        var id = await sender.Send(command, ct);
        return Created($"/payment-types/controller/{id}", new { id });
    }

    [HttpPut("{id:int}")]
    public async Task Update(int id, UpdatePaymentTypeCommand command, CancellationToken ct)
    {
        command.Id = id;
        await sender.Send(command, ct);
    }

    [HttpDelete("{id:int}")]
    public async Task Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeletePaymentTypeCommand(id), ct);
    }

    [HttpPut("{id:int}/enable")]
    public async Task Enable(int id, CancellationToken ct)
    {
        await sender.Send(new EnablePaymentTypeCommand(id), ct);
    }

    [HttpPut("{id:int}/disable")]
    public async Task Disable(int id, CancellationToken ct)
    {
        await sender.Send(new DisablePaymentTypeCommand(id), ct);
    }

    [HttpGet("{id:int}")]
    public async Task<GetPaymentTypeByIdQueryDto> GetById(int id, CancellationToken ct)
    {
        return await sender.Send(new GetPaymentTypeByIdQuery(id), ct);
    }

    [HttpGet]
    public async Task<PageResult<ListPaymentTypesQueryDto>> List([FromQuery] ListPaymentTypesQuery query, CancellationToken ct)
    {
        return await sender.Send(query, ct);
    }
}
