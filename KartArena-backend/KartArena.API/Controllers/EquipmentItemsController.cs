using KartArena.Application.Modules.Catalog.EquipmentItems.Commands.Create;
using KartArena.Application.Modules.Catalog.EquipmentItems.Commands.Delete;
using KartArena.Application.Modules.Catalog.EquipmentItems.Commands.Update;
using KartArena.Application.Modules.Catalog.EquipmentItems.Queries.GetById;
using KartArena.Application.Modules.Catalog.EquipmentItems.Queries.List;
using Microsoft.AspNetCore.Authorization;

namespace KartArena.API.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    [ApiController]
    [Route("equipmentItems/controller")]
    public class EquipmentItemsController(ISender sender) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<int>> CreateEquipmentItem(
            CreateEquipmentItemCommand command,
            CancellationToken ct)
        {
            if (command == null)
                return BadRequest("Invalid request body.");

            int id = await sender.Send(command, ct);

            return Created("", new { id });
        }

        [HttpPut("{id:int}")]
        public async Task UpdateEquipment(
            int id,
            UpdateEquipmentItemCommand command,
            CancellationToken ct)
        {
            command.Id = id;
            await sender.Send(command, ct);
        }

        [HttpDelete("{id:int}")]
        public async Task DeleteEquipment(
            int id,
            CancellationToken ct)
        {
            await sender.Send(
                new DeleteEquipmentItemCommand(id),
                ct);
        }

        [HttpGet("{id:int}")]
        public async Task<GetEquipmentItemByIdQueryDto> GetById(
            int id,
            CancellationToken ct)
        {
            var equipment = await sender.Send(
                new GetEquipmentItemByIdQuery(id),
                ct);

            return equipment;
        }

        [HttpGet]
        public async Task<PageResult<ListEquipmentItemsQueryDto>> List(
            [FromQuery] ListEquipmentItemsQuery query,
            CancellationToken ct)
        {
            var result = await sender.Send(query, ct);

            return result;
        }
    }
}