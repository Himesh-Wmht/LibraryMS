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
    public sealed class UserContactRepository
    {
        private readonly SqlDb _db;
        public UserContactRepository(SqlDb db) => _db = db;

        public async Task<UserContactDto?> GetByUserCodeAsync(string userCode)
        {
            const string sql = @"
SELECT U_CODE, ISNULL(U_NAME,''), ISNULL(U_EMAIL,'')
FROM dbo.M_TBLUSERS
WHERE U_CODE = @U;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();

            if (await r.ReadAsync())
            {
                return new UserContactDto(
                    UserCode: r.GetString(0),
                    UserName: r.GetString(1),
                    Email: r.GetString(2)
                );
            }

            return null;
        }
    }

    public sealed record UserContactDto(
        string UserCode,
        string UserName,
        string Email
    );
}
