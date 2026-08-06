using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KartArena.Application.Modules.Reports;

public sealed class PdfReportService
{
    public byte[] Generate(string title, string filters, IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyList<string>> rows) => Document.Create(document =>
    {
        document.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(24);
            page.DefaultTextStyle(x => x.FontSize(8).FontFamily(Fonts.Arial));
            page.Header().Column(column =>
            {
                column.Item().Text(title).FontSize(20).Bold().FontColor(Colors.Red.Darken1);
                column.Item().PaddingTop(4).Text($"Generated: {DateTime.Now:dd.MM.yyyy HH:mm}");
                column.Item().Text($"Filters: {filters}").FontColor(Colors.Grey.Darken2);
            });
            page.Content().PaddingVertical(14).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    for (var i = 0; i < headers.Count; i++) columns.RelativeColumn();
                });
                table.Header(header =>
                {
                    foreach (var value in headers)
                        header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text(value).FontColor(Colors.White).SemiBold();
                });
                foreach (var row in rows)
                    foreach (var value in row)
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(value ?? string.Empty);
            });
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Page "); text.CurrentPageNumber(); text.Span(" of "); text.TotalPages();
            });
        });
    }).GeneratePdf();
}
