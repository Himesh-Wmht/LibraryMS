using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;

namespace LibraryMS.DAL.Repositories
{
    public sealed class UserRegistrationRepository
    {
        private readonly SqlDb _db;
        public UserRegistrationRepository(SqlDb db) => _db = db;

        public async Task InsertUserAsync(SqlConnection con, SqlTransaction tx, UserInsertRow u)
        {
            const string sql = @"
                                INSERT INTO dbo.M_TBLUSERS
                                (
                                  U_CODE, U_NAME, U_ACTIVE, U_GROUP, U_MOBILE, U_DOB, U_ADDRESS, U_PASSWORD, U_NIC, M_DATE,
                                  U_UID, U_GENDER, U_MEMSTATUS, U_SUBSSTATUS, U_EMAIL, U_REGISTEREDATE, U_SUBSTYPE, U_EXPIREDDATE,
                                  U_MAXBORROW
                                )
                                VALUES
                                (
                                  @U_CODE, @U_NAME, @U_ACTIVE, @U_GROUP, @U_MOBILE, @U_DOB, @U_ADDRESS, @U_PASSWORD, @U_NIC, SYSDATETIME(),
                                  @U_UID, @U_GENDER, @U_MEMSTATUS, @U_SUBSSTATUS, @U_EMAIL, @U_REGISTEREDATE, @U_SUBSTYPE, @U_EXPIREDDATE,
                                  @U_MAXBORROW
                                );";

            await using var cmd = new SqlCommand(sql, con, tx);

            cmd.Parameters.Add("@U_CODE", SqlDbType.VarChar, 20).Value = u.Code;
            cmd.Parameters.Add("@U_NAME", SqlDbType.NVarChar, 120).Value = u.Name;
            cmd.Parameters.Add("@U_ACTIVE", SqlDbType.Bit).Value = u.Active;
            cmd.Parameters.Add("@U_GROUP", SqlDbType.VarChar, 20).Value = u.GroupCode;
            cmd.Parameters.Add("@U_MOBILE", SqlDbType.VarChar, 20).Value = u.Mobile;

            cmd.Parameters.Add("@U_DOB", SqlDbType.Date).Value = (object?)u.Dob ?? DBNull.Value;
            cmd.Parameters.Add("@U_ADDRESS", SqlDbType.NVarChar, 250).Value = (object?)u.Address ?? DBNull.Value;

            cmd.Parameters.Add("@U_PASSWORD", SqlDbType.NVarChar, 255).Value = u.PasswordHash;
            cmd.Parameters.Add("@U_NIC", SqlDbType.VarChar, 20).Value = (object?)u.Nic ?? DBNull.Value;

            cmd.Parameters.Add("@U_UID", SqlDbType.VarChar, 20).Value = u.Uid;
            cmd.Parameters.Add("@U_GENDER", SqlDbType.VarChar, 20).Value = (object?)u.Gender ?? DBNull.Value;

            cmd.Parameters.Add("@U_MEMSTATUS", SqlDbType.Bit).Value = u.MemberStatus;
            cmd.Parameters.Add("@U_SUBSSTATUS", SqlDbType.Bit).Value = u.SubscriptionStatus;

            cmd.Parameters.Add("@U_EMAIL", SqlDbType.NVarChar, 120).Value = (object?)u.Email ?? DBNull.Value;

            cmd.Parameters.Add("@U_REGISTEREDATE", SqlDbType.DateTime).Value = u.RegisterDate;
            cmd.Parameters.Add("@U_SUBSTYPE", SqlDbType.NVarChar, 120).Value = (object?)u.SubscriptionType ?? DBNull.Value;
            cmd.Parameters.Add("@U_EXPIREDDATE", SqlDbType.DateTime).Value = (object?)u.ExpireDate ?? DBNull.Value;

            cmd.Parameters.Add("@U_MAXBORROW", SqlDbType.Int).Value = u.MaxBorrow;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task InsertUserLocationsAllAsync(SqlConnection con, SqlTransaction tx, string userCode, bool active)
        {
            const string sql = @"
INSERT INTO dbo.M_TBLUSERLOCATION (UL_USERCODE, UL_USERLOC, UL_ACTIVE, M_DATE)
SELECT @U, L_CODE, @A, SYSDATETIME()
FROM dbo.M_LOCATION
WHERE L_ACTIVE = 1;";

            await using var cmd = new SqlCommand(sql, con, tx);
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;
            cmd.Parameters.Add("@A", SqlDbType.Bit).Value = active;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task InsertUserLocationOneAsync(SqlConnection con, SqlTransaction tx, string userCode, string locCode, bool active)
        {
            const string sql = @"
INSERT INTO dbo.M_TBLUSERLOCATION (UL_USERCODE, UL_USERLOC, UL_ACTIVE, M_DATE)
VALUES (@U, @L, @A, SYSDATETIME());";

            await using var cmd = new SqlCommand(sql, con, tx);
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;
            cmd.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = locCode;
            cmd.Parameters.Add("@A", SqlDbType.Bit).Value = active;

            await cmd.ExecuteNonQueryAsync();
        }
    }

    public record UserInsertRow(
        string Code,
        string Name,
        bool Active,
        string GroupCode,
        string Mobile,
        DateTime? Dob,
        string? Address,
        string PasswordHash,
        string? Nic,
        string Uid,
        string? Gender,
        bool MemberStatus,
        bool SubscriptionStatus,
        string? Email,
        DateTime RegisterDate,
        string? SubscriptionType,
        DateTime? ExpireDate,
        int MaxBorrow
    );
}
