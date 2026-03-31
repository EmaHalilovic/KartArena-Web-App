using KartArena.Application.Common;
using MediatR;

namespace KartArena.Application.Modules.Catalog.Karts.Queries.List
{
    public sealed class ListKartsQuery : IRequest<PageResult<ListKartsQueryDto>>
    {
        public string? Search { get; init; }
        public bool? OnlyEnabled { get; init; }
        public PageRequest Paging { get; init; } = new();
    }
}
