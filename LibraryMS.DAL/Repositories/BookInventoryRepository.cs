using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public sealed class BookInventoryRepository
    {
        private readonly SqlDb _db;
        public BookInventoryRepository(SqlDb db) => _db = db;
        public async Task<List<LookupItemDto>> LookupBooksAsync(string? text)
        {
            const string sql = @"
                        SELECT TOP (200)
                            b.B_CODE,
                            b.B_TITLE,
                            ISNULL(c.BC_NAME,'')
                        FROM dbo.M_TBLBOOKS b
                        LEFT JOIN dbo.M_TBLBOOKCATEGORY c ON c.BC_CODE = b.B_CATEGORY
                        WHERE ISNULL(b.B_ACTIVE,0)=1
                          AND (@T IS NULL OR b.B_CODE LIKE '%' + @T + '%'
                                       OR b.B_TITLE LIKE '%' + @T + '%'
                                       OR ISNULL(b.B_ISBN,'') LIKE '%' + @T + '%')
                        ORDER BY b.B_TITLE;";

            var list = new List<LookupItemDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@T", SqlDbType.NVarChar, 200).Value = (object?)NullIfEmpty(text) ?? DBNull.Value;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new LookupItemDto(
                    Code: r.GetString(0),
                    Name: r.GetString(1),
                    Extra: r.IsDBNull(2) ? null : r.GetString(2)
                ));
            }
            return list;
        }
        public async Task<List<InvRowDto>> SearchAsync(string locCode, string? text, bool activeOnly)
        {
            const string sql = @"
                        SELECT
                          i.BI_ID,
                          i.BI_BOOKCODE,
                          b.B_TITLE,
                          i.BI_LOCCODE,
                          ISNULL(i.BI_QTY,0),
                          ISNULL(i.BI_REORDER,0),
                          ISNULL(i.BI_ACTIVE,0)
                        FROM dbo.T_TBLBOOKINVENTORY i
                        INNER JOIN dbo.M_TBLBOOKS b ON b.B_CODE = i.BI_BOOKCODE
                        WHERE i.BI_LOCCODE=@L
                          AND (@T IS NULL OR i.BI_BOOKCODE LIKE '%' + @T + '%'
                                     OR b.B_TITLE LIKE '%' + @T + '%')
                          AND (@AO=0 OR ISNULL(i.BI_ACTIVE,0)=1)
                        ORDER BY b.B_TITLE;";

            var list = new List<InvRowDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = locCode;
            cmd.Parameters.Add("@T", SqlDbType.NVarChar, 200).Value = (object?)NullIfEmpty(text) ?? DBNull.Value;
            cmd.Parameters.Add("@AO", SqlDbType.Bit).Value = activeOnly;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new InvRowDto(
                    Id: r.GetInt32(0),
                    BookCode: r.GetString(1),
                    Title: r.GetString(2),
                    LocCode: r.GetString(3),
                    Qty: r.GetInt32(4),
                    Reorder: r.GetInt32(5),
                    Active: r.GetBoolean(6)
                ));
            }
            return list;
        }
        public async Task<string?> GetBookTitleAsync(string bookCode)
        {
            const string sql = @"
                                SELECT TOP 1 B_TITLE
                                FROM dbo.M_TBLBOOKS
                                WHERE B_CODE=@B AND ISNULL(B_ACTIVE,0)=1;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = bookCode;

            await con.OpenAsync();
            var obj = await cmd.ExecuteScalarAsync();

            return obj == null || obj == DBNull.Value ? null : obj.ToString();
        }
        // SET qty + reorder (simple)
        public async Task UpsertAsync(InvUpsertDto dto)
        {
            const string sql = @"
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

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = dto.BookCode;
            cmd.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = dto.LocCode;
            cmd.Parameters.Add("@Q", SqlDbType.Int).Value = dto.Qty;
            cmd.Parameters.Add("@R", SqlDbType.Int).Value = dto.Reorder;
            cmd.Parameters.Add("@A", SqlDbType.Bit).Value = dto.Active;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
