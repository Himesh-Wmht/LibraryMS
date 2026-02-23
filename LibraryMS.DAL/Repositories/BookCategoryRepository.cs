using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public sealed class BookCategoryRepository
    {
        private readonly SqlDb _db;
        public BookCategoryRepository(SqlDb db) => _db = db;

        // Used by Book Catalog lookup dropdown
        public async Task<List<BookCategoryDto>> GetActiveAsync()
        {
            const string sql = @"
                            SELECT BC_CODE, BC_NAME, ISNULL(BC_ACTIVE,0)
                            FROM dbo.M_TBLBOOKCATEGORY
                            WHERE ISNULL(BC_ACTIVE,0)=1
                            ORDER BY BC_NAME;";

            var list = new List<BookCategoryDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new BookCategoryDto(
                    Code: r.GetString(0),
                    Name: r.GetString(1),
                    Active: r.GetBoolean(2)
                ));
            }
            return list;
        }

        // ✅ Category page grid
        public async Task<List<BookCategoryRowDto>> SearchAsync(string? text, bool activeOnly)
        {
            const string sql = @"
                                SELECT BC_CODE, BC_NAME, ISNULL(BC_ACTIVE,0)
                                FROM dbo.M_TBLBOOKCATEGORY
                                WHERE (@T IS NULL OR BC_CODE LIKE '%' + @T + '%'
                                               OR BC_NAME LIKE '%' + @T + '%')
                                  AND (@AO = 0 OR ISNULL(BC_ACTIVE,0)=1)
                                ORDER BY BC_NAME;";

            var list = new List<BookCategoryRowDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@T", SqlDbType.NVarChar, 200).Value = (object?)NullIfEmpty(text) ?? DBNull.Value;
            cmd.Parameters.Add("@AO", SqlDbType.Bit).Value = activeOnly;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new BookCategoryRowDto(
                    Code: r.GetString(0),
                    Name: r.GetString(1),
                    Active: r.GetBoolean(2)
                ));
            }
            return list;
        }

        // ✅ Create/Update (Upsert)
        public async Task UpsertAsync(BookCategoryUpsertDto dto)
        {
            const string sql = @"
                            IF EXISTS (SELECT 1 FROM dbo.M_TBLBOOKCATEGORY WHERE BC_CODE=@Code)
                            BEGIN
                              UPDATE dbo.M_TBLBOOKCATEGORY
                              SET BC_NAME=@Name,
                                  BC_ACTIVE=@Active,
                                  M_DATE=SYSDATETIME()
                              WHERE BC_CODE=@Code;
                            END
                            ELSE
                            BEGIN
                              INSERT INTO dbo.M_TBLBOOKCATEGORY (BC_CODE, BC_NAME, BC_ACTIVE, M_DATE)
                              VALUES (@Code, @Name, @Active, SYSDATETIME());
                            END;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@Code", SqlDbType.VarChar, 20).Value = dto.Code;
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 120).Value = dto.Name;
            cmd.Parameters.Add("@Active", SqlDbType.Bit).Value = dto.Active;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
