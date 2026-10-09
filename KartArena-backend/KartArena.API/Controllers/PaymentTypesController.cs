using KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Create;
using KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Delete;
using KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Status.Disable;
using KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Status.Enable;
using KartArena.Application.Modules.Catalog.PaymentTypes.Commands.Update;
using KartArena.Application.Modules.Catalog.PaymentTypes.Queries.GetById;
using KartArena.Application.Modules.Catalog.PaymentTypes.Queries.List;
using Microsoft.AspNetCore.Authorization;

namespace KartArena.API.Controllers;

[ApiController]
[Route("payment-types/controller")]
public sealed class PaymentTypesController(ISender sender) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<int>> Create(
        CreatePaymentTypeCommand command,
        CancellationToken ct)
    {
        if (command == null)
            return BadRequest("Invalid request body.");

        var id = await sender.Send(command, ct);

        return Created($"/payment-types/controller/{id}", new { id });
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpPut("{id:int}")]
    public async Task Update(
        int id,
        UpdatePaymentTypeCommand command,
        CancellationToken ct)
    {
        command.Id = id;
        await sender.Send(command, ct);
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpDelete("{id:int}")]
    public async Task Delete(
        int id,
        CancellationToken ct)
    {
        await sender.Send(new DeletePaymentTypeCommand(id), ct);
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpPut("{id:int}/enable")]
    public async Task Enable(
        int id,
        CancellationToken ct)
    {
        await sender.Send(new EnablePaymentTypeCommand(id), ct);
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpPut("{id:int}/disable")]
    public async Task Disable(
        int id,
        CancellationToken ct)
    {
        await sender.Send(new DisablePaymentTypeCommand(id), ct);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<GetPaymentTypeByIdQueryDto> GetById(
        int id,
        CancellationToken ct)
    {
        return await sender.Send(
            new GetPaymentTypeByIdQuery(id),
            ct);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<PageResult<ListPaymentTypesQueryDto>> List(
        [FromQuery] ListPaymentTypesQuery query,
        CancellationToken ct)
    {
        return await sender.Send(query, ct);
    }
}
