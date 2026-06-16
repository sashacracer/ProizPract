using System.Data;
using System.IO;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace WarehouseAccountingApp.Services;

public static class ExportService
{
    static ExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // ── Excel ─────────────────────────────────────────────────────────────
    public static void ExportToExcel(DataTable table, string title, string filePath)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Отчёт");

        // Title row
        ws.Cell(1, 1).Value = title;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Range(1, 1, 1, table.Columns.Count).Merge();

        // Date row
        ws.Cell(2, 1).Value = $"Сформирован: {DateTime.Now:dd.MM.yyyy HH:mm}";
        ws.Cell(2, 1).Style.Font.Italic = true;
        ws.Range(2, 1, 2, table.Columns.Count).Merge();

        // Headers
        for (int c = 0; c < table.Columns.Count; c++)
        {
            var cell = ws.Cell(4, c + 1);
            cell.Value = table.Columns[c].ColumnName;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1A4E9E");
            cell.Style.Font.FontColor = XLColor.FromHtml("#A8C8F8");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#2E2E38");
        }

        // Data
        for (int r = 0; r < table.Rows.Count; r++)
        {
            for (int c = 0; c < table.Columns.Count; c++)
            {
                var cell = ws.Cell(r + 5, c + 1);
                var val = table.Rows[r][c];
                cell.Value = val == DBNull.Value ? "" : val.ToString()!;

                if (r % 2 == 1)
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E1E24");

                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Hair;
                cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#2E2E38");
            }
        }

        ws.Columns().AdjustToContents();
        wb.SaveAs(filePath);
    }

    // ── PDF ───────────────────────────────────────────────────────────────
    public static void ExportToPdf(DataTable table, string title, string filePath)
    {
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);

                page.Header().Column(col =>
                {
                    col.Item().Text(title).FontSize(16).Bold();
                    col.Item().Text($"Сформирован: {DateTime.Now:dd.MM.yyyy HH:mm}")
                       .FontSize(9).FontColor("#8E8E9A");
                    col.Item().Height(8);
                });

                page.Content().Table(t =>
                {
                    // Columns
                    t.ColumnsDefinition(def =>
                    {
                        for (int i = 0; i < table.Columns.Count; i++)
                            def.RelativeColumn();
                    });

                    // Header
                    foreach (DataColumn col in table.Columns)
                    {
                        t.Header(h =>
                        {
                            h.Cell().Background("#1A4E9E").Padding(4)
                             .Text(col.ColumnName).FontSize(8).Bold().FontColor("#A8C8F8");
                        });
                    }

                    // Rows
                    for (int r = 0; r < table.Rows.Count; r++)
                    {
                        var bg = r % 2 == 0 ? "#1A1A1F" : "#1E1E24";
                        foreach (DataColumn col in table.Columns)
                        {
                            var val = table.Rows[r][col];
                            t.Cell().Background(bg).BorderBottom(0.5f).BorderColor("#2E2E38").Padding(4)
                             .Text(val == DBNull.Value ? "" : val.ToString()!).FontSize(8).FontColor("#C8C8D0");
                        }
                    }
                });

                page.Footer().AlignRight()
                    .Text(x =>
                    {
                        x.Span("Стр. ").FontSize(8).FontColor("#6B6B72");
                        x.CurrentPageNumber().FontSize(8).FontColor("#6B6B72");
                        x.Span(" из ").FontSize(8).FontColor("#6B6B72");
                        x.TotalPages().FontSize(8).FontColor("#6B6B72");
                    });
            });
        }).GeneratePdf(filePath);
    }

    // ── Save dialog helpers ───────────────────────────────────────────────
    public static string? PickExcelPath(string defaultName)
    {
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            FileName = defaultName,
            DefaultExt = ".xlsx",
            Filter = "Excel файл (*.xlsx)|*.xlsx"
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public static string? PickPdfPath(string defaultName)
    {
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            FileName = defaultName,
            DefaultExt = ".pdf",
            Filter = "PDF файл (*.pdf)|*.pdf"
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }
}
