using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public sealed class UserLookupRepository
    {
        private readonly SqlDb _db;
        public UserLookupRepository(SqlDb db) => _db = db;

        public async Task<List<LookupItemDto>> LookupUsersAsync(string? text)
        {
            const string sql = @"
                    SELECT TOP (200) U_CODE, ISNULL(U_NAME,''), ISNULL(U_MOBILE,'')
                    FROM dbo.M_TBLUSERS
                    WHERE ISNULL(U_ACTIVE,0)=1
                      AND (@T IS NULL OR U_CODE LIKE '%' + @T + '%'
                                   OR U_NAME LIKE '%' + @T + '%'
                                   OR ISNULL(U_MOBILE,'') LIKE '%' + @T + '%')
                    ORDER BY U_CODE;";

            var list = new List<LookupItemDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@T", SqlDbType.NVarChar, 200).Value = (object?)NullIfEmpty(text) ?? DBNull.Value;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(new LookupItemDto(r.GetString(0), r.GetString(1), r.IsDBNull(2) ? null : r.GetString(2)));

            return list;
        }

        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}