using MediatR;

namespace KartArena.Application.Modules.Catalog.Karts.Commands.Status.Disable
{
    public sealed class DisableKartCommand : IRequest<Unit>
    {
        public required int Id { get; set; }
    }
}
