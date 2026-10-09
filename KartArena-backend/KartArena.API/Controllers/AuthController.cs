using KartArena.Application.Modules.Auth.Commands.Login;
using KartArena.Application.Modules.Auth.Commands.Logout;
using KartArena.Application.Modules.Auth.Commands.Refresh;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginCommandDto>> Login(
        [FromBody] LoginCommand command,
        CancellationToken ct)
    {
        return Ok(await mediator.Send(command, ct));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshTokenCommandDto>> Refresh(
        [FromBody] RefreshTokenCommand command,
        CancellationToken ct)
    {
        return Ok(await mediator.Send(command, ct));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task Logout(
        [FromBody] LogoutCommand command,
        CancellationToken ct)
    {
        await mediator.Send(command, ct);
    }
}