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
