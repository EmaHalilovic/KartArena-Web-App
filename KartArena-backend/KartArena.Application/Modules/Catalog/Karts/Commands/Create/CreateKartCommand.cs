using KartArena.Domain.Entities.Catalog;
using MediatR;

namespace KartArena.Application.Modules.Catalog.Karts.Commands.Create
{
    public class CreateKartCommand : IRequest<int>
    {
        public string? Name { get; set; }
        public string? Colour { get; set; }
        public int? YearOfManufacture { get; set; }
        public string? ChassisNumber { get; set; }
        public string? Manufacturer { get; set; }

        public string? ImageUrl { get; set; }
        public decimal? PricePerSession { get; set; }
        public string? Description { get; set; }

        public int? PowertrainTypeId { get; set; }
    }
}