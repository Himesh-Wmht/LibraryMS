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

    public record UserRow(string U_CODE, string U_NAME, bool U_ACTIVE, string U_GROUP, string U_PASSWORD, int FailCount=0, bool Locked=false );
    public sealed record LoginUserDb(string UserCode, string UserName, string GroupCode, bool Active, string PasswordStored, int FailCount, bool Locked);

    public sealed class UserRepository
    {
        private readonly SqlDb _db;
        public UserRepository(SqlDb db) => _db = db;
        public async Task<LoginUserDb?> GetLoginUserAsync(string userCode)
        {
            const string sql = @"
SELECT U_CODE, U_NAME, U_GROUP, ISNULL(U_ACTIVE,0), U_PASSWORD, ISNULL(U_FAIL_COUNT,0), ISNULL(U_LOCKED,0)
FROM dbo.M_TBLUSERS
WHERE U_CODE=@U;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return null;

            return new LoginUserDb(
                UserCode: r.GetString(0),
                UserName: r.GetString(1),
                GroupCode: r.GetString(2),
                Active: r.GetBoolean(3),
                PasswordStored: r.GetString(4),
                FailCount: r.GetInt32(5),
                Locked: r.GetBoolean(6)
            );
        }

        public async Task OnLoginSuccessAsync(string userCode, string? upgradedHash = null)
        {
            const string sql = @"
UPDATE dbo.M_TBLUSERS
SET U_FAIL_COUNT=0,
    U_LOCKED=0,
    U_LOCKED_AT=NULL,
    U_PASSWORD=COALESCE(@H, U_PASSWORD),
    M_DATE=SYSDATETIME()
WHERE U_CODE=@U;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;
            cmd.Parameters.Add("@H", SqlDbType.NVarChar, 200).Value = (object?)upgradedHash ?? DBNull.Value;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<(int FailCount, bool LockedNow)> OnLoginFailAsync(string userCode)
        {
            const string sql = @"
UPDATE dbo.M_TBLUSERS
SET U_FAIL_COUNT = ISNULL(U_FAIL_COUNT,0) + 1,
    U_LOCKED = CASE WHEN ISNULL(U_FAIL_COUNT,0) + 1 >= 4 THEN 1 ELSE ISNULL(U_LOCKED,0) END,
    U_LOCKED_AT = CASE WHEN ISNULL(U_FAIL_COUNT,0) + 1 >= 4 THEN SYSDATETIME() ELSE U_LOCKED_AT END,
    M_DATE = SYSDATETIME()
OUTPUT inserted.U_FAIL_COUNT, inserted.U_LOCKED
WHERE U_CODE=@U;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return (0, false);

            return (r.GetInt32(0), r.GetBoolean(1));
        }
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
