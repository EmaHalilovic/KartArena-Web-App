using MediatR;

namespace KartArena.Application.Modules.Catalog.Karts.Commands.Delete
{
    public class DeleteKartCommand : IRequest<Unit>
    {
        public required int Id { get; set; }
    }
}
