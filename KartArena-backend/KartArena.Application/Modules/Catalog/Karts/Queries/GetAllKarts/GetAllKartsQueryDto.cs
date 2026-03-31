namespace KartArena.Application.Modules.Catalog.Karts.Queries.GetAll
{
    public class GetAllKartsQueryDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Colour { get; set; }
        public int? YearOfManufacture { get; set; }
        public string? ChassisNumber { get; set; }
        public string? Manufacturer { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? PricePerSession { get; set; }
        public string? Description { get; set; }
        public string? PowertrainTypeName { get; set; }
        public bool IsEnabled { get; set; }
    }
}