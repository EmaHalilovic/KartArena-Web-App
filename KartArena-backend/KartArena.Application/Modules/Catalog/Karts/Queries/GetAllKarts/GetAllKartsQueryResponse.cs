using System.Collections.Generic;

namespace KartArena.Application.Modules.Catalog.Karts.Queries.GetAll
{
    public class GetAllKartsQueryResponse
    {
        public int TotalCount { get; set; }
        public List<GetAllKartsQueryDto> Items { get; set; } = new();
    }
}
