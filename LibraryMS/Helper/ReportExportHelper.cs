using System;
using System.Data;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LibraryMS.Win.Helper
{
    public static class ReportExportHelper
    {
        public static void ExportToExcel(DataTable dt, string filePath, string sheetName = "Report")
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(dt, sheetName);

            ws.Columns().AdjustToContents();
            ws.Row(1).Style.Font.Bold = true;
            ws.SheetView.FreezeRows(1);

            wb.SaveAs(filePath);
        }

        public static void ExportToPdf(DataTable dt, string filePath, string title)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header()
                        .Column(col =>
                        {
                            col.Item().Text(title).Bold().FontSize(16);
                            col.Item().Text($"Generated On: {DateTime.Now:yyyy-MM-dd HH:mm:ss}").FontSize(9);
                        });

                    page.Content()
                        .Table(table =>
                        {
                            var colCount = dt.Columns.Count;

                            table.ColumnsDefinition(columns =>
                            {
                                for (int i = 0; i < colCount; i++)
                                    columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                foreach (DataColumn col in dt.Columns)
                                {
                                    header.Cell()
                                        .Background(Colors.Grey.Lighten2)
                                        .Border(1)
                                        .Padding(4)
                                        .Text(col.ColumnName)
                                        .Bold();
                                }
                            });

                            foreach (DataRow row in dt.Rows)
                            {
                                foreach (DataColumn col in dt.Columns)
                                {
                                    var text = row[col] == DBNull.Value ? "" : Convert.ToString(row[col]) ?? "";

                                    table.Cell()
                                        .Border(1)
                                        .Padding(4)
                                        .Text(text);
                                }
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf(filePath);
        }
    }
}