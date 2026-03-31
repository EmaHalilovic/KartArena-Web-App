using MediatR;

namespace KartArena.Application.Modules.Catalog.Karts.Commands.Status.Enable
{
    public sealed class EnableKartCommand : IRequest<Unit>
    {
        public required int Id { get; set; }
    }
}
