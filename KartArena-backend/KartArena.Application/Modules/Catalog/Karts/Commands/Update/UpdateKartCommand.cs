using MediatR;
using System.Text.Json.Serialization;

namespace KartArena.Application.Modules.Catalog.Karts.Commands.Update
{
    public sealed class UpdateKartCommand : IRequest<Unit>
    {
        [JsonIgnore]
        public int Id { get; set; }

        public required string Name { get; set; }

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
