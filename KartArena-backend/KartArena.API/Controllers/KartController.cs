using KartArena.Application.Common;
using KartArena.Application.Modules.Catalog.Karts.Commands.Create;
using KartArena.Application.Modules.Catalog.Karts.Commands.Update;
using KartArena.Application.Modules.Catalog.Karts.Commands.Delete;
using KartArena.Application.Modules.Catalog.Karts.Commands.Status.Disable;
using KartArena.Application.Modules.Catalog.Karts.Commands.Status.Enable;
using KartArena.Application.Modules.Catalog.Karts.Queries.GetById;
using KartArena.Application.Modules.Catalog.Karts.Queries.List;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KartArena.API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("[controller]")]
    public class KartsController(ISender sender) : ControllerBase
    {
        /// <summary>
        /// Creates a new kart.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<int>> CreateKart(CreateKartCommand command, CancellationToken ct)
        {
            int id = await sender.Send(command, ct);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        /// <summary>
        /// Updates an existing kart.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task Update(int id, UpdateKartCommand command, CancellationToken ct)
        {
            // Route ID always takes precedence.
            command.Id = id;
            await sender.Send(command, ct);
            // No return → 204 No Content
        }

        /// <summary>
        /// Deletes a kart by ID.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task Delete(int id, CancellationToken ct)
        {
            await sender.Send(new DeleteKartCommand { Id = id }, ct);
            // No return → 204 No Content
        }

        /// <summary>
        /// Gets a kart by ID.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<GetKartByIdQueryDto> GetById(int id, CancellationToken ct)
        {
            var kart = await sender.Send(new GetKartByIdQuery { Id = id }, ct);
            return kart; // If not found → handled via middleware
        }

        /// <summary>
        /// Lists all karts (paged and filtered).
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<PageResult<ListKartsQueryDto>> List([FromQuery] ListKartsQuery query, CancellationToken ct)
        {
            var result = await sender.Send(query, ct);
            return result;
        }

        /// <summary>
        /// Disables a kart (sets IsEnabled = false).
        /// </summary>
        [HttpPut("{id:int}/disable")]
        public async Task Disable(int id, CancellationToken ct)
        {
            await sender.Send(new DisableKartCommand { Id = id }, ct);
            // No return → 204 No Content
        }

        /// <summary>
        /// Enables a kart (sets IsEnabled = true).
        /// </summary>
        [HttpPut("{id:int}/enable")]
        public async Task Enable(int id, CancellationToken ct)
        {
            await sender.Send(new EnableKartCommand { Id = id }, ct);
            // No return → 204 No Content
        }
    }
}
