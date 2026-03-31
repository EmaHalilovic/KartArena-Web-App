using MediatR;

namespace KartArena.Application.Modules.Catalog.Karts.Queries.GetById
{
    public class GetKartByIdQuery : IRequest<GetKartByIdQueryDto>
    {
        public int Id { get; set; }
    }
}
