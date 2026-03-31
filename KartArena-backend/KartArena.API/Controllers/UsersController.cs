using KartArena.Application.Modules.Catalog.Users.Commands.Create;
using KartArena.Application.Modules.Catalog.Users.Commands.Update;
using KartArena.Application.Modules.Catalog.Users.Commands.Delete;
using KartArena.Application.Modules.Catalog.Users.Commands.Status.Disable;
using KartArena.Application.Modules.Catalog.Users.Commands.Status.Enable;

namespace KartArena.API.Controllers
    {
        [ApiController]
        [Route("api/users")]
        public class UsersController(ISender sender) : ControllerBase
        {
            [HttpPost]
            public async Task<ActionResult<int>> CreateUser(CreateUserCommand command, CancellationToken ct)
            {
                if (command == null)
                    return BadRequest("Invalid request body.");

                int id = await sender.Send(command, ct);

                return Created("", new { id });
            }
        // ---------------------------------------------------------------
            [HttpDelete("{id:int}")]
            public async Task Delete(int id, CancellationToken cancellationToken)
            {
                await sender.Send(new DeleteUserCommand { Id = id }, cancellationToken);
            }
        // ---------------------------------------------------------------
            [HttpPut("{id:int}")]
            public async Task Update(int id, UpdateUserCommand command, CancellationToken cancellationToken)
            {
                command.Id = id;
                await sender.Send(command, cancellationToken);
            }
        // ---------------------------------------------------------------
            [HttpPut("{id:int}/disable")]
            public async Task Disable(int id, CancellationToken cancellationToken)
            {
                await sender.Send(new DisableUserCommand { Id = id }, cancellationToken);
            }

        // ---------------------------------------------------------------
            [HttpPut("{id:int}/enable")]
            public async Task Enable(int id, CancellationToken cancellationToken)
            {
                await sender.Send(new EnableUserCommand { Id = id }, cancellationToken);
            }
    }
}

