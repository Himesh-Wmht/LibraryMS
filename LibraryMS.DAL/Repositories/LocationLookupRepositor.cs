using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public sealed class LocationLookupRepository
    {

        private readonly SqlDb _db;
        public LocationLookupRepository(SqlDb db) => _db = db;

        // ✅ matches service call
        public async Task<List<LookupItemDto>> LookupAsync(string? text)
        {
            const string sql = @"
                SELECT TOP (200) L_CODE, L_DESC
                FROM dbo.M_LOCATION
                WHERE ISNULL(L_ACTIVE,1)=1
                  AND (@T IS NULL OR L_CODE LIKE '%' + @T + '%'
                               OR L_DESC LIKE '%' + @T + '%')
                ORDER BY L_DESC;";

            var list = new List<LookupItemDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@T", SqlDbType.NVarChar, 200).Value = (object?)NullIfEmpty(text) ?? DBNull.Value;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(new LookupItemDto(r.GetString(0), r.GetString(1)));

            return list;
        }
        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    }
}