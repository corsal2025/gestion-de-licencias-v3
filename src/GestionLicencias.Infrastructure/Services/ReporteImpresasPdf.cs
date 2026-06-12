using GestionLicencias.Core.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestionLicencias.Infrastructure.Services;

/// <summary>
/// "Impresas" PDF report: licenses printed by a given clerk, headed by the
/// service location (Placilla, Av. Argentina or Mercado Puerto). Includes
/// full name, RUT and the print date taken from the FECHA IMPRESION column.
/// </summary>
public static class ReporteImpresasPdf
{
    public static byte[] Generar(IReadOnlyList<TramiteLicencia> registros, string lugar, string funcionaria)
    {
        var filas = registros
            .Where(r => string.IsNullOrWhiteSpace(funcionaria)
                ? string.Equals(r.LugarAtencion, lugar, StringComparison.OrdinalIgnoreCase)
                : string.Equals(r.ImpresaPor, funcionaria, StringComparison.OrdinalIgnoreCase))
            .Select(r => new { r.NombreCompleto, r.RUT, r.FechaImpresion })
            .OrderByDescending(f => f.FechaImpresion.HasValue)
            .ThenByDescending(f => f.FechaImpresion)
            .ThenBy(f => f.NombreCompleto)
            .ToList();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor("#1c2333"));

                page.Header().Row(row =>
                {
                    row.ConstantItem(86).Padding(2).Svg(LogoValpo.Svg);
                    row.RelativeItem().PaddingLeft(12).Column(col =>
                    {
                        col.Item().Text("DIRECCIÓN DE TRÁNSITO — GESTIÓN DE LICENCIAS")
                            .FontSize(8).FontColor("#5b6478").LetterSpacing(0.08f);
                        col.Item().PaddingTop(2).Text($"REPORTE DE IMPRESAS — {lugar.ToUpperInvariant()}")
                            .FontSize(16).Bold().FontColor("#0b1326");
                        col.Item().PaddingTop(2)
                            .Text($"Funcionaria: {(string.IsNullOrWhiteSpace(funcionaria) ? "TODAS" : funcionaria)} — " +
                                  $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm} — {filas.Count} registro(s)")
                            .FontSize(8).FontColor("#5b6478");
                        col.Item().PaddingTop(6).LineHorizontal(2).LineColor("#0e69dc");
                    });
                });

                page.Content().PaddingVertical(12).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);
                        columns.RelativeColumn(3f);
                        columns.RelativeColumn(1.4f);
                        columns.RelativeColumn(1.6f);
                    });

                    table.Header(header =>
                    {
                        static IContainer Th(IContainer c) =>
                            c.Background("#0b1326").PaddingVertical(5).PaddingHorizontal(6);

                        header.Cell().Element(Th).Text("#").Bold().FontColor("#ffffff").FontSize(8);
                        header.Cell().Element(Th).Text("NOMBRE COMPLETO").Bold().FontColor("#ffffff").FontSize(8);
                        header.Cell().Element(Th).Text("RUT").Bold().FontColor("#ffffff").FontSize(8);
                        header.Cell().Element(Th).Text("FECHA IMPRESIÓN").Bold().FontColor("#ffffff").FontSize(8);
                    });

                    var i = 0;
                    foreach (var f in filas)
                    {
                        i++;
                        var bg = i % 2 == 0 ? "#eef1f8" : "#ffffff";

                        IContainer Td(IContainer c) =>
                            c.Background(bg).PaddingVertical(4).PaddingHorizontal(6);

                        table.Cell().Element(Td).Text(i.ToString()).FontColor("#5b6478").FontSize(8);
                        table.Cell().Element(Td).Text(f.NombreCompleto);
                        table.Cell().Element(Td).Text(f.RUT);
                        table.Cell().Element(Td).Text(f.FechaImpresion?.ToString("dd/MM/yyyy") ?? "—")
                            .FontColor(f.FechaImpresion.HasValue ? "#1c2333" : "#9aa1b5");
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.DefaultTextStyle(s => s.FontSize(8).FontColor("#5b6478"));
                    t.Span("Página ");
                    t.CurrentPageNumber();
                    t.Span(" de ");
                    t.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}
