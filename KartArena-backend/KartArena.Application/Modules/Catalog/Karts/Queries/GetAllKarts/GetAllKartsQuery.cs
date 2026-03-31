using MediatR;

namespace KartArena.Application.Modules.Catalog.Karts.Queries.GetAll
{
    public class GetAllKartsQuery : IRequest<GetAllKartsQueryResponse>
    {
        public string? Search { get; set; }
        public bool? IsEnabled { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
