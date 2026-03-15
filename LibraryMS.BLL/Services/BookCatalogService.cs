using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class BookCatalogService
    {
        private readonly BookCatalogRepository _books;
        private readonly BookCategoryRepository _cats;

        public BookCatalogService(BookCatalogRepository books, BookCategoryRepository cats)
        {
            _books = books;
            _cats = cats;
        }

        public Task<List<BookCategoryDto>> GetCategoriesAsync() => _cats.GetActiveAsync();

        public Task<List<BookRowDto>> SearchAsync(string? text, string? category, bool activeOnly)
            => _books.SearchAsync(text, category, activeOnly);

        public Task SaveAsync(BookUpsertDto dto) => _books.UpsertAsync(dto);

        public async Task<BookCatalogImportResultDto> ImportExcelAsync(string filePath)
        {
            var result = new BookCatalogImportResultDto();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                result.Errors.Add("Excel file not found.");
                return result;
            }

            try
            {
                using var wb = new XLWorkbook(filePath);
                var ws = wb.Worksheets.FirstOrDefault();

                if (ws == null)
                {
                    result.Errors.Add("Workbook does not contain any worksheet.");
                    return result;
                }

                var lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
                var lastCol = ws.LastColumnUsed()?.ColumnNumber() ?? 0;

                if (lastRow < 2)
                {
                    result.Errors.Add("Excel does not contain data rows.");
                    return result;
                }

                var headers = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
                for (int c = 1; c <= lastCol; c++)
                {
                    var h = ws.Cell(1, c).GetString().Trim();
                    if (!string.IsNullOrWhiteSpace(h) && !headers.ContainsKey(h))
                        headers.Add(h, c);
                }

                string[] required =
                {
                    "BOOK_CODE",
                    "TITLE",
                    "CATEGORY_CODE",
                    "PRICE",
                    "BOOK_ACTIVE",
                    "LOCATION_CODE",
                    "QTY",
                    "REORDER",
                    "INV_ACTIVE"
                };

                foreach (var req in required)
                {
                    if (!headers.ContainsKey(req))
                        result.Errors.Add($"Missing Excel column: {req}");
                }

                if (result.Errors.Count > 0)
                    return result;

                var rows = new List<BookCatalogImportRowDto>();

                for (int r = 2; r <= lastRow; r++)
                {
                    bool emptyRow = true;
                    for (int c = 1; c <= lastCol; c++)
                    {
                        if (!string.IsNullOrWhiteSpace(ws.Cell(r, c).GetString()))
                        {
                            emptyRow = false;
                            break;
                        }
                    }

                    if (emptyRow) continue;

                    try
                    {
                        var row = new BookCatalogImportRowDto
                        {
                            ExcelRowNo = r,
                            BookCode = GetString(ws, r, headers, "BOOK_CODE"),
                            Title = GetString(ws, r, headers, "TITLE"),
                            Author = NullIfEmpty(GetString(ws, r, headers, "AUTHOR", false)),
                            Publisher = NullIfEmpty(GetString(ws, r, headers, "PUBLISHER", false)),
                            Isbn = NullIfEmpty(GetString(ws, r, headers, "ISBN", false)),
                            CategoryCode = GetString(ws, r, headers, "CATEGORY_CODE"),
                            Price = GetDecimal(ws, r, headers, "PRICE"),
                            Active = GetBool(ws, r, headers, "BOOK_ACTIVE", true),
                            LocationCode = GetString(ws, r, headers, "LOCATION_CODE"),
                            Qty = GetInt(ws, r, headers, "QTY"),
                            Reorder = GetInt(ws, r, headers, "REORDER"),
                            InventoryActive = GetBool(ws, r, headers, "INV_ACTIVE", true)
                        };

                        rows.Add(row);
                    }
                    catch (System.Exception ex)
                    {
                        result.Errors.Add($"Row {r}: {ex.Message}");
                    }
                }

                if (result.Errors.Count > 0)
                    return result;

                return await _books.ImportExcelAsync(rows);
            }
            catch (System.Exception ex)
            {
                result.Errors.Add($"Excel read failed: {ex.Message}");
                return result;
            }
        }

        private static string GetString(IXLWorksheet ws, int row, Dictionary<string, int> headers, string columnName, bool required = true)
        {
            if (!headers.TryGetValue(columnName, out var col))
            {
                if (required) throw new System.Exception($"{columnName} column not found.");
                return string.Empty;
            }

            var value = ws.Cell(row, col).GetString().Trim();

            if (required && string.IsNullOrWhiteSpace(value))
                throw new System.Exception($"{columnName} is required.");

            return value;
        }

        private static int GetInt(IXLWorksheet ws, int row, Dictionary<string, int> headers, string columnName)
        {
            var s = GetString(ws, row, headers, columnName);

            if (!int.TryParse(s, out var value))
                throw new System.Exception($"{columnName} must be a valid integer.");

            return value;
        }

        private static decimal GetDecimal(IXLWorksheet ws, int row, Dictionary<string, int> headers, string columnName)
        {
            var s = GetString(ws, row, headers, columnName);

            if (!decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) &&
                !decimal.TryParse(s, out value))
                throw new System.Exception($"{columnName} must be a valid decimal.");

            return value;
        }

        private static bool GetBool(IXLWorksheet ws, int row, Dictionary<string, int> headers, string columnName, bool defaultValue)
        {
            if (!headers.TryGetValue(columnName, out var col))
                return defaultValue;

            var s = ws.Cell(row, col).GetString().Trim();

            if (string.IsNullOrWhiteSpace(s))
                return defaultValue;

            if (s.Equals("1") || s.Equals("true", System.StringComparison.OrdinalIgnoreCase) ||
                s.Equals("yes", System.StringComparison.OrdinalIgnoreCase) || s.Equals("y", System.StringComparison.OrdinalIgnoreCase) ||
                s.Equals("active", System.StringComparison.OrdinalIgnoreCase))
                return true;

            if (s.Equals("0") || s.Equals("false", System.StringComparison.OrdinalIgnoreCase) ||
                s.Equals("no", System.StringComparison.OrdinalIgnoreCase) || s.Equals("n", System.StringComparison.OrdinalIgnoreCase) ||
                s.Equals("inactive", System.StringComparison.OrdinalIgnoreCase))
                return false;

            throw new System.Exception($"{columnName} must be TRUE/FALSE, YES/NO, Y/N, 1/0.");
        }

        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}