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
    public sealed class NotificationRepository
    {
        private readonly SqlDb _db;
        public NotificationRepository(SqlDb db) => _db = db;

        public async Task QueueAsync(
            string eventType,
            string? refDocNo,
            string? userCode,
            string? emailTo,
            string subject,
            string body,
            string channel)
        {
            const string sql = @"
                    INSERT INTO dbo.T_TBLNOTIFICATION_OUTBOX
                    (NO_EVENT_TYPE, NO_REF_DOCNO, NO_USERCODE, NO_EMAIL_TO, NO_SUBJECT, NO_BODY, NO_CHANNEL, NO_STATUS, NO_CREATED_AT, M_DATE)
                    VALUES
                    (@E, @R, @U, @T, @S, @B, @C, 'P', SYSDATETIME(), SYSDATETIME());";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@E", SqlDbType.VarChar, 40).Value = eventType;
            cmd.Parameters.Add("@R", SqlDbType.VarChar, 30).Value = (object?)refDocNo ?? DBNull.Value;
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = (object?)userCode ?? DBNull.Value;
            cmd.Parameters.Add("@T", SqlDbType.VarChar, 200).Value = (object?)emailTo ?? DBNull.Value;
            cmd.Parameters.Add("@S", SqlDbType.NVarChar, 200).Value = subject;
            cmd.Parameters.Add("@B", SqlDbType.NVarChar).Value = body;
            cmd.Parameters.Add("@C", SqlDbType.VarChar, 20).Value = channel;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<NotificationOutboxRow>> GetPendingAsync(int maxRows = 20)
        {
            const string sql = @"
            SELECT TOP (@M)
                NO_ID, NO_EVENT_TYPE, NO_REF_DOCNO, NO_USERCODE, NO_EMAIL_TO,
                NO_SUBJECT, NO_BODY, NO_CHANNEL, NO_RETRY_COUNT
            FROM dbo.T_TBLNOTIFICATION_OUTBOX
            WHERE NO_STATUS = 'P'
            ORDER BY NO_ID;";

            var list = new List<NotificationOutboxRow>();

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@M", SqlDbType.Int).Value = maxRows;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();

            while (await r.ReadAsync())
            {
                list.Add(new NotificationOutboxRow(
                    Id: r.GetInt32(0),
                    EventType: r.GetString(1),
                    RefDocNo: r.IsDBNull(2) ? null : r.GetString(2),
                    UserCode: r.IsDBNull(3) ? null : r.GetString(3),
                    EmailTo: r.IsDBNull(4) ? null : r.GetString(4),
                    Subject: r.GetString(5),
                    Body: r.GetString(6),
                    Channel: r.GetString(7),
                    RetryCount: r.GetInt32(8)
                ));
            }

            return list;
        }

        public async Task MarkSentAsync(int id)
        {
            const string sql = @"
                UPDATE dbo.T_TBLNOTIFICATION_OUTBOX
                SET NO_STATUS='S',
                    NO_SENT_AT=SYSDATETIME(),
                    M_DATE=SYSDATETIME()
                WHERE NO_ID=@I;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@I", SqlDbType.Int).Value = id;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task MarkFailedAsync(int id, string error)
        {
            const string sql = @"
                UPDATE dbo.T_TBLNOTIFICATION_OUTBOX
                SET NO_STATUS='F',
                    NO_RETRY_COUNT = ISNULL(NO_RETRY_COUNT,0) + 1,
                    NO_ERROR=@E,
                    M_DATE=SYSDATETIME()
                WHERE NO_ID=@I;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@I", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@E", SqlDbType.NVarChar, 500).Value = error;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddInAppAsync(string userCode, string title, string message, string? refDocNo, string eventType)
        {
            const string sql = @"
                    INSERT INTO dbo.T_TBLUSER_NOTIFICATIONS
                    (UN_USERCODE, UN_TITLE, UN_MESSAGE, UN_REF_DOCNO, UN_EVENT_TYPE, UN_IS_READ, UN_CREATED_AT, M_DATE)
                    VALUES
                    (@U, @T, @M, @R, @E, 0, SYSDATETIME(), SYSDATETIME());";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;
            cmd.Parameters.Add("@T", SqlDbType.NVarChar, 200).Value = title;
            cmd.Parameters.Add("@M", SqlDbType.NVarChar).Value = message;
            cmd.Parameters.Add("@R", SqlDbType.VarChar, 30).Value = (object?)refDocNo ?? DBNull.Value;
            cmd.Parameters.Add("@E", SqlDbType.VarChar, 40).Value = eventType;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }

    public sealed record NotificationOutboxRow(
        int Id,
        string EventType,
        string? RefDocNo,
        string? UserCode,
        string? EmailTo,
        string Subject,
        string Body,
        string Channel,
        int RetryCount
    );
}
