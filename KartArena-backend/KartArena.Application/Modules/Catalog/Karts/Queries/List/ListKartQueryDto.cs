namespace KartArena.Application.Modules.Catalog.Karts.Queries.List
{
    public sealed class ListKartsQueryDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Colour { get; set; }
        public string? Manufacturer { get; set; }
        public bool IsEnabled { get; set; }
        public string? ImageUrl { get; set; }
        public decimal? PricePerSession { get; set; }
        public string? Description { get; set; }
    }
}
