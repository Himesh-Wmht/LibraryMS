using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public sealed class PasswordResetRequestRepository
    {
        private readonly SqlDb _db;
        public PasswordResetRequestRepository(SqlDb db) => _db = db;

        public async Task<List<PwdResetRowDto>> GetPendingAsync()
        {
            const string sql = @"
                                SELECT r.PR_ID,
                                       r.PR_USERCODE,
                                       ISNULL(u.U_NAME,''),
                                       u.U_MOBILE,
                                       r.PR_REQ_DATE,
                                       r.PR_STATUS,
                                       r.PR_BY
                                FROM dbo.T_TBLPWDRESETREQ r
                                LEFT JOIN dbo.M_TBLUSERS u ON u.U_CODE = r.PR_USERCODE
                                WHERE r.PR_STATUS='P'
                                ORDER BY r.PR_REQ_DATE DESC;";

            return await ReadAsync(sql);
        }

        public async Task<List<PwdResetRowDto>> GetAllAsync()
        {
            const string sql = @"
                                SELECT r.PR_ID,
                                       r.PR_USERCODE,
                                       ISNULL(u.U_NAME,''),
                                       u.U_MOBILE,
                                       r.PR_REQ_DATE,
                                       r.PR_STATUS,
                                       r.PR_BY
                                FROM dbo.T_TBLPWDRESETREQ r
                                LEFT JOIN dbo.M_TBLUSERS u ON u.U_CODE = r.PR_USERCODE
                                ORDER BY r.PR_REQ_DATE DESC;";

            return await ReadAsync(sql);
        }

        private async Task<List<PwdResetRowDto>> ReadAsync(string sql)
        {
            var list = new List<PwdResetRowDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new PwdResetRowDto(
                    PrId: r.GetString(0),
                    UserCode: r.GetString(1),
                    Name: r.GetString(2),
                    Mobile: r.IsDBNull(3) ? null : r.GetString(3),
                    ReqDate: r.GetDateTime(4),
                    Status: r.GetString(5),
                    RequestedBy: r.IsDBNull(6) ? null : r.GetString(6)
                ));
            }
            return list;
        }

        // ✅ Insert request with PLAIN password into PR_REQ_HASH (as you requested)
        public async Task CreateRequestAsync(string userCode, string plainNewPassword, string? requestedBy)
        {
            var prId = "PR" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            const string sql = @"
                                INSERT INTO dbo.T_TBLPWDRESETREQ
                                (PR_ID, PR_USERCODE, PR_REQ_HASH, PR_REQ_DATE, PR_STATUS, PR_BY, M_DATE)
                                VALUES
                                (@I, @U, @P, SYSDATETIME(), 'P', @B, SYSDATETIME());";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@I", SqlDbType.VarChar, 30).Value = prId;
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;
            cmd.Parameters.Add("@P", SqlDbType.NVarChar, 200).Value = plainNewPassword; // stored plain
            cmd.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = (object?)requestedBy ?? DBNull.Value;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ✅ Fetch plain password for hashing during approval
        public async Task<(string UserCode, string PlainPassword)> GetForApproveAsync(string prId)
        {
            const string sql = @"
                                SELECT PR_USERCODE, PR_REQ_HASH
                                FROM dbo.T_TBLPWDRESETREQ
                                WHERE PR_ID=@I AND PR_STATUS='P';";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@I", SqlDbType.VarChar, 30).Value = prId;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();

            if (!await r.ReadAsync())
                throw new Exception("Request not found or already processed.");

            var userCode = r.GetString(0);
            var plain = r.GetString(1);

            return (userCode, plain);
        }

        // ✅ Approve: update user hashed password + clear plain request password
        public async Task ApproveAsync(string prId, string adminUserCode, string hashedPassword)
        {
            const string sql = @"
                                BEGIN TRAN;

                                UPDATE dbo.M_TBLUSERS
                                SET U_PASSWORD = @H,
                                    U_FAIL_COUNT = 0,
                                    U_LOCKED = 0,
                                    U_LOCKED_AT = NULL,
                                    U_GRACEDATE = 0
                                    M_DATE = SYSDATETIME()
                                    U_TEMPDATETIME = SYSDATETIME()
                                WHERE U_CODE = (SELECT PR_USERCODE FROM dbo.T_TBLPWDRESETREQ WHERE PR_ID=@I AND PR_STATUS='P');

                                IF @@ROWCOUNT = 0
                                BEGIN
                                  ROLLBACK;
                                  RAISERROR('Approve failed: user not found or request not pending.', 16, 1);
                                  RETURN;
                                END

                                UPDATE dbo.T_TBLPWDRESETREQ
                                SET PR_STATUS='A',
                                    PR_APP_BY=@B,
                                    PR_APP_DATE=SYSDATETIME(),
                                    M_DATE=SYSDATETIME()
                                WHERE PR_ID=@I AND PR_STATUS='P';

                                COMMIT;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.CommandTimeout = 120;

            cmd.Parameters.Add("@I", SqlDbType.VarChar, 30).Value = prId;
            cmd.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = adminUserCode;
            cmd.Parameters.Add("@H", SqlDbType.NVarChar, 200).Value = hashedPassword;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RejectAsync(string prId, string adminUserCode, string? remark)
        {
            const string sql = @"
                                UPDATE dbo.T_TBLPWDRESETREQ
                                SET PR_STATUS='R',
                                    PR_REMARK=@R,
                                    PR_APP_BY=@B,
                                    PR_APP_DATE=SYSDATETIME(),
                                    M_DATE=SYSDATETIME()
                                WHERE PR_ID=@I AND PR_STATUS='P';";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@I", SqlDbType.VarChar, 30).Value = prId;
            cmd.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = adminUserCode;
            cmd.Parameters.Add("@R", SqlDbType.NVarChar, 200).Value = (object?)remark ?? DBNull.Value;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}