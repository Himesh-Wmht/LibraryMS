using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public sealed class UserLockApprovalRepository
    {
        private readonly SqlDb _db;
        public UserLockApprovalRepository(SqlDb db) => _db = db;

        // ✅ Create 1 pending unlock request per user
        public async Task EnsurePendingUnlockRequestAsync(string userCode)
        {
            var ulId = "UL" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            const string sql = @"
                                IF NOT EXISTS (SELECT 1 FROM dbo.T_TBLUSERLOCKAPPROVAL WHERE UL_USERCODE=@U AND UL_STATUS='P')
                                BEGIN
                                  INSERT INTO dbo.T_TBLUSERLOCKAPPROVAL (UL_ID, UL_USERCODE, UL_DATE, UL_STATUS, M_DATE)
                                  VALUES (@I, @U, SYSDATETIME(), 'P', SYSDATETIME());
                                END;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@I", SqlDbType.VarChar, 30).Value = ulId;
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ✅ Admin page: list pending unlock requests
        public async Task<List<UserLockRowDto>> GetPendingAsync()
        {
            const string sql = @"
                                SELECT UL_ID, UL_USERCODE, UL_DATE, UL_STATUS
                                FROM dbo.T_TBLUSERLOCKAPPROVAL
                                WHERE UL_STATUS='P'
                                ORDER BY UL_DATE DESC;";

            var list = new List<UserLockRowDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new UserLockRowDto(
                    UlId: r.GetString(0),
                    UserCode: r.GetString(1),
                    LockedAt: r.GetDateTime(2),
                    Status: r.GetString(3)
                ));
            }
            return list;
        }

        // ✅ Admin page: unlock
        public async Task UnlockAsync(string ulId, string adminUserCode)
        {
            const string sql = @"
                                BEGIN TRAN;

                                DECLARE @U varchar(20);

                                SELECT @U = UL_USERCODE
                                FROM dbo.T_TBLUSERLOCKAPPROVAL
                                WHERE UL_ID=@I AND UL_STATUS='P';

                                IF @U IS NULL
                                BEGIN
                                  ROLLBACK;
                                  RAISERROR('Unlock failed: request not pending.', 16, 1);
                                  RETURN;
                                END

                                UPDATE dbo.M_TBLUSERS
                                SET U_FAIL_COUNT=0,
                                    U_LOCKED=0,
                                    U_LOCKED_AT=NULL,
                                    M_DATE=SYSDATETIME()
                                WHERE U_CODE=@U;

                                UPDATE dbo.T_TBLUSERLOCKAPPROVAL
                                SET UL_STATUS='U',
                                    UL_BY=@B,
                                    UL_BY_DATE=SYSDATETIME(),
                                    M_DATE=SYSDATETIME()
                                WHERE UL_ID=@I AND UL_STATUS='P';

                                COMMIT;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.CommandTimeout = 120;

            cmd.Parameters.Add("@I", SqlDbType.VarChar, 30).Value = ulId;
            cmd.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = adminUserCode;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}