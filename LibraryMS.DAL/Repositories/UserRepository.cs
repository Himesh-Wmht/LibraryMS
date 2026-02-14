using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;

namespace LibraryMS.DAL.Repositories
{

    public record UserRow(string U_CODE, string U_NAME, bool U_ACTIVE, string U_GROUP, string U_PASSWORD);

    public sealed class UserRepository
    {
        private readonly SqlDb _db;
        public UserRepository(SqlDb db) => _db = db;

        public async Task<UserRow?> GetUserByCodeAsync(string userCode)
        {
            const string sql = @"
                                SELECT U_CODE, U_NAME, U_ACTIVE, U_GROUP, U_PASSWORD
                                FROM M_TBLUSERS
                                WHERE U_CODE = @U_CODE;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@U_CODE", SqlDbType.VarChar, 20).Value = userCode;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();

            if (!await r.ReadAsync())
                return null;

            return new UserRow(
                r.GetString(0),
                r.GetString(1),
                Convert.ToBoolean(r["U_ACTIVE"]),
                r.GetString(3),
                r.IsDBNull(4) ? "" : r.GetString(4)
            );
        }

        public async Task UpdatePasswordAsync(string userCode, string newPasswordHash)
        {
            const string sql = @"
                                UPDATE M_TBLUSERS
                                SET U_PASSWORD = @PWD, M_DATE = SYSDATETIME()
                                WHERE U_CODE = @U_CODE;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@PWD", SqlDbType.NVarChar, 300).Value = newPasswordHash;
            cmd.Parameters.Add("@U_CODE", SqlDbType.VarChar, 20).Value = userCode;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
