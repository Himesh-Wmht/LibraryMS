using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public sealed class BookCatalogRepository
    {
        private readonly SqlDb _db;
        public BookCatalogRepository(SqlDb db) => _db = db;

        public async Task<List<BookRowDto>> SearchAsync(string? text, string? category, bool activeOnly)
        {
            const string sql = @"
                    SELECT
                      b.B_CODE, b.B_TITLE, b.B_AUTHOR, b.B_PUBLISHER, b.B_ISBN,
                      b.B_CATEGORY, c.BC_NAME,
                      ISNULL(b.B_PRICE,0), ISNULL(b.B_ACTIVE,0)
                    FROM dbo.M_TBLBOOKS b
                    LEFT JOIN dbo.M_TBLBOOKCATEGORY c ON c.BC_CODE = b.B_CATEGORY
                    WHERE (@T IS NULL OR b.B_CODE LIKE '%' + @T + '%'
                                 OR b.B_TITLE LIKE '%' + @T + '%'
                                 OR ISNULL(b.B_ISBN,'') LIKE '%' + @T + '%')
                      AND (@C IS NULL OR b.B_CATEGORY = @C)
                      AND (@AO = 0 OR ISNULL(b.B_ACTIVE,0)=1)
                    ORDER BY b.B_TITLE;";

            var list = new List<BookRowDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@T", SqlDbType.NVarChar, 200).Value = (object?)NullIfEmpty(text) ?? DBNull.Value;
            cmd.Parameters.Add("@C", SqlDbType.VarChar, 20).Value = (object?)NullIfEmpty(category) ?? DBNull.Value;
            cmd.Parameters.Add("@AO", SqlDbType.Bit).Value = activeOnly;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new BookRowDto(
                    Code: r.GetString(0),
                    Title: r.GetString(1),
                    Author: r.IsDBNull(2) ? null : r.GetString(2),
                    Publisher: r.IsDBNull(3) ? null : r.GetString(3),
                    Isbn: r.IsDBNull(4) ? null : r.GetString(4),
                    CategoryCode: r.IsDBNull(5) ? null : r.GetString(5),
                    CategoryName: r.IsDBNull(6) ? null : r.GetString(6),
                    Price: r.IsDBNull(7) ? 0m : r.GetDecimal(7),
                    Active: r.GetBoolean(8)
                ));
            }
            return list;
        }
        public async Task<BookCatalogImportResultDto> ImportExcelAsync(List<BookCatalogImportRowDto> rows)
        {
            var result = new BookCatalogImportResultDto
            {
                FileRowCount = rows?.Count ?? 0
            };

            if (rows == null || rows.Count == 0)
            {
                result.Errors.Add("No rows found in Excel file.");
                return result;
            }

            // Validate same book code has same master data across rows
            foreach (var grp in rows.GroupBy(x => x.BookCode, StringComparer.OrdinalIgnoreCase))
            {
                var first = grp.First();

                bool mismatch = grp.Any(x =>
                    !string.Equals((x.Title ?? "").Trim(), (first.Title ?? "").Trim(), StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals((x.Author ?? "").Trim(), (first.Author ?? "").Trim(), StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals((x.Publisher ?? "").Trim(), (first.Publisher ?? "").Trim(), StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals((x.Isbn ?? "").Trim(), (first.Isbn ?? "").Trim(), StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals((x.CategoryCode ?? "").Trim(), (first.CategoryCode ?? "").Trim(), StringComparison.OrdinalIgnoreCase) ||
                    x.Price != first.Price ||
                    x.Active != first.Active
                );

                if (mismatch)
                {
                    result.Errors.Add($"Book Code '{grp.Key}' has inconsistent catalog details across multiple Excel rows.");
                }
            }

            if (result.Errors.Count > 0)
                return result;

            const string sqlBookUpsert = @"
IF EXISTS (SELECT 1 FROM dbo.M_TBLBOOKS WHERE B_CODE=@Code)
BEGIN
  UPDATE dbo.M_TBLBOOKS
  SET B_TITLE=@Title,
      B_AUTHOR=@Author,
      B_PUBLISHER=@Publisher,
      B_ISBN=@Isbn,
      B_CATEGORY=@Category,
      B_PRICE=@Price,
      B_ACTIVE=@Active,
      M_DATE=SYSDATETIME()
  WHERE B_CODE=@Code;
END
ELSE
BEGIN
  INSERT INTO dbo.M_TBLBOOKS
  (B_CODE,B_TITLE,B_AUTHOR,B_PUBLISHER,B_ISBN,B_CATEGORY,B_PRICE,B_ACTIVE,M_DATE)
  VALUES
  (@Code,@Title,@Author,@Publisher,@Isbn,@Category,@Price,@Active,SYSDATETIME());
END;";

            const string sqlInventoryUpsert = @"
IF EXISTS (SELECT 1 FROM dbo.T_TBLBOOKINVENTORY WHERE BI_BOOKCODE=@B AND BI_LOCCODE=@L)
BEGIN
  UPDATE dbo.T_TBLBOOKINVENTORY
  SET BI_QTY=@Q, BI_REORDER=@R, BI_ACTIVE=@A, M_DATE=SYSDATETIME()
  WHERE BI_BOOKCODE=@B AND BI_LOCCODE=@L;
END
ELSE
BEGIN
  INSERT INTO dbo.T_TBLBOOKINVENTORY (BI_BOOKCODE, BI_LOCCODE, BI_QTY, BI_REORDER, BI_ACTIVE, M_DATE)
  VALUES(@B,@L,@Q,@R,@A,SYSDATETIME());
END;";

            try
            {
                await using var con = _db.CreateConnection();
                await con.OpenAsync();
                await using var tx = (SqlTransaction)await con.BeginTransactionAsync();

                var validCats = await LoadCodeSetAsync(
                    con, tx,
                    "SELECT BC_CODE FROM dbo.M_TBLBOOKCATEGORY WHERE BC_CODE IS NOT NULL");

                var validLocs = await LoadCodeSetAsync(
                    con, tx,
                    "SELECT L_CODE FROM dbo.M_LOCATION WHERE L_CODE IS NOT NULL");

                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row.BookCode))
                        result.Errors.Add($"Row {row.ExcelRowNo}: Book Code is required.");

                    if (string.IsNullOrWhiteSpace(row.Title))
                        result.Errors.Add($"Row {row.ExcelRowNo}: Title is required.");

                    if (string.IsNullOrWhiteSpace(row.CategoryCode))
                        result.Errors.Add($"Row {row.ExcelRowNo}: Category Code is required.");
                    else if (!validCats.Contains(row.CategoryCode.Trim()))
                        result.Errors.Add($"Row {row.ExcelRowNo}: Category '{row.CategoryCode}' doesn't match category table.");

                    if (string.IsNullOrWhiteSpace(row.LocationCode))
                        result.Errors.Add($"Row {row.ExcelRowNo}: Location Code is required.");
                    else if (!validLocs.Contains(row.LocationCode.Trim()))
                        result.Errors.Add($"Row {row.ExcelRowNo}: Location '{row.LocationCode}' doesn't match location table.");

                    if (row.Qty < 0)
                        result.Errors.Add($"Row {row.ExcelRowNo}: Qty cannot be negative.");

                    if (row.Reorder < 0)
                        result.Errors.Add($"Row {row.ExcelRowNo}: Reorder cannot be negative.");

                    if (row.Price < 0)
                        result.Errors.Add($"Row {row.ExcelRowNo}: Price cannot be negative.");
                }

                if (result.Errors.Count > 0)
                {
                    await tx.RollbackAsync();
                    return result;
                }

                // Upsert catalog once per book code
                foreach (var book in rows
                    .GroupBy(x => x.BookCode, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First()))
                {
                    await using var cmd = new SqlCommand(sqlBookUpsert, con, tx);

                    cmd.Parameters.Add("@Code", SqlDbType.NVarChar, 50).Value = book.BookCode.Trim();
                    cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 255).Value = book.Title.Trim();
                    cmd.Parameters.Add("@Author", SqlDbType.NVarChar, 255).Value = (object?)book.Author ?? DBNull.Value;
                    cmd.Parameters.Add("@Publisher", SqlDbType.NVarChar, 255).Value = (object?)book.Publisher ?? DBNull.Value;
                    cmd.Parameters.Add("@Isbn", SqlDbType.NVarChar, 100).Value = (object?)book.Isbn ?? DBNull.Value;
                    cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 50).Value = book.CategoryCode.Trim();

                    var pPrice = cmd.Parameters.Add("@Price", SqlDbType.Decimal);
                    pPrice.Precision = 18;
                    pPrice.Scale = 2;
                    pPrice.Value = book.Price;

                    cmd.Parameters.Add("@Active", SqlDbType.Bit).Value = book.Active;

                    await cmd.ExecuteNonQueryAsync();
                    result.CatalogUpsertCount++;
                }

                // Upsert inventory for every row
                foreach (var row in rows)
                {
                    await using var cmd = new SqlCommand(sqlInventoryUpsert, con, tx);

                    cmd.Parameters.Add("@B", SqlDbType.NVarChar, 50).Value = row.BookCode.Trim();
                    cmd.Parameters.Add("@L", SqlDbType.NVarChar, 50).Value = row.LocationCode.Trim();
                    cmd.Parameters.Add("@Q", SqlDbType.Int).Value = row.Qty;
                    cmd.Parameters.Add("@R", SqlDbType.Int).Value = row.Reorder;
                    cmd.Parameters.Add("@A", SqlDbType.Bit).Value = row.InventoryActive;

                    await cmd.ExecuteNonQueryAsync();
                    result.InventoryUpsertCount++;
                }

                await tx.CommitAsync();
                result.Ok = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Import failed: {ex.Message}");
                return result;
            }
        }

        private static async Task<HashSet<string>> LoadCodeSetAsync(SqlConnection con, SqlTransaction tx, string sql)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            await using var cmd = new SqlCommand(sql, con, tx);
            await using var r = await cmd.ExecuteReaderAsync();

            while (await r.ReadAsync())
            {
                if (!r.IsDBNull(0))
                    set.Add(r.GetString(0).Trim());
            }

            return set;
        }
        public async Task UpsertAsync(BookUpsertDto dto)
        {
            const string sql = @"
                    IF EXISTS (SELECT 1 FROM dbo.M_TBLBOOKS WHERE B_CODE=@Code)
                    BEGIN
                      UPDATE dbo.M_TBLBOOKS
                      SET B_TITLE=@Title,
                          B_AUTHOR=@Author,
                          B_PUBLISHER=@Publisher,
                          B_ISBN=@Isbn,
                          B_CATEGORY=@Category,
                          B_PRICE=@Price,
                          B_ACTIVE=@Active,
                          M_DATE=SYSDATETIME()
                      WHERE B_CODE=@Code;
                    END
                    ELSE
                    BEGIN
                      INSERT INTO dbo.M_TBLBOOKS
                      (B_CODE,B_TITLE,B_AUTHOR,B_PUBLISHER,B_ISBN,B_CATEGORY,B_PRICE,B_ACTIVE,M_DATE)
                      VALUES
                      (@Code,@Title,@Author,@Publisher,@Isbn,@Category,@Price,@Active,SYSDATETIME());
                    END;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@Code", SqlDbType.VarChar, 20).Value = dto.Code;
            cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = dto.Title;
            cmd.Parameters.Add("@Author", SqlDbType.NVarChar, 120).Value = (object?)dto.Author ?? DBNull.Value;
            cmd.Parameters.Add("@Publisher", SqlDbType.NVarChar, 120).Value = (object?)dto.Publisher ?? DBNull.Value;
            cmd.Parameters.Add("@Isbn", SqlDbType.NVarChar, 40).Value = (object?)dto.Isbn ?? DBNull.Value;
            cmd.Parameters.Add("@Category", SqlDbType.VarChar, 20).Value = (object?)dto.CategoryCode ?? DBNull.Value;

            var p = cmd.Parameters.Add("@Price", SqlDbType.Decimal);
            p.Precision = 18; p.Scale = 2; p.Value = dto.Price;

            cmd.Parameters.Add("@Active", SqlDbType.Bit).Value = dto.Active;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
