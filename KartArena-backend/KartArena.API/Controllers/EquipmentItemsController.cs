using KartArena.Application.Modules.Catalog.Equipment.Commands.Create;
using KartArena.Application.Modules.Catalog.Equipment.Commands.Delete;
using KartArena.Application.Modules.Catalog.Equipment.Commands.Status.Disable;
using KartArena.Application.Modules.Catalog.Equipment.Commands.Status.Enable;
using KartArena.Application.Modules.Catalog.Equipment.Commands.Update;
using KartArena.Application.Modules.Catalog.Equipment.Queries.GetById;
using KartArena.Application.Modules.Catalog.Equipment.Queries.List;
using KartArena.Application.Modules.Catalog.EquipmentItems.Commands.Create;
using KartArena.Application.Modules.Catalog.EquipmentItems.Commands.Delete;
using KartArena.Application.Modules.Catalog.EquipmentItems.Commands.Update;
using KartArena.Application.Modules.Catalog.EquipmentItems.Queries.GetById;
using KartArena.Application.Modules.Catalog.EquipmentItems.Queries.List;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace KartArena.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("equipmentItems/controller")]
    public class EquipmentItemsController(ISender sender) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<int>> CreateEquipmentItem(CreateEquipmentItemCommand command, CancellationToken ct)
        {
            if (command == null)
                return BadRequest("Invalid request body.");
            int id = await sender.Send(command, ct);
            return Created("", new { id });
        }

        [HttpPut("{id:int}")]
        public async Task UpdateEquipment(int id, UpdateEquipmentItemCommand command, CancellationToken ct)
        {
            command.Id = id;
            await sender.Send(command, ct);
        }

        [HttpDelete("{id:int}")]
        public async Task DeleteEquipment(int id, CancellationToken ct)
        {
            await sender.Send(new DeleteEquipmentItemCommand(id), ct);
        }

        //[HttpPut("{id:int}/enable")]
        //public async Task EnableEquipment(int id, CancellationToken ct)
        //{
        //    await sender.Send(new EnableEquipmentCommand(id), ct);
        //}

        //[HttpPut("{id:int}/disable")]
        //public async Task DisableEquipment(int id, CancellationToken ct)
        //{
        //    await sender.Send(new DisableEquipmentCommand(id), ct);
        //}

        [HttpGet("{id:int}")]
        public async Task<GetEquipmentItemByIdQueryDto> GetById(int id, CancellationToken ct)
        {
            var equipment = await sender.Send(new GetEquipmentItemByIdQuery(id), ct);
            return equipment; // if NotFoundException -> 404 via middleware
        }

        [HttpGet]
        public async Task<PageResult<ListEquipmentItemsQueryDto>> List([FromQuery] ListEquipmentItemsQuery query, CancellationToken ct)
        {
            var result = await sender.Send(query, ct);
            return result;
        }


    }
}
