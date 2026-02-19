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
        public record UserSearchRow(string Code, string Name);

        public record UserDetailsRow(
            string Code,
            string Name,
            bool Active,
            string GroupCode,
            string Mobile,
            DateTime? Dob,
            string? Address,
            string? Nic,
            string? Email,
            string Uid,
            string? Gender,
            bool MemberStatus,
            bool SubscriptionStatus,
            string? SubscriptionType,
            DateTime? ExpireDate,
            int MaxBorrow
        );

        public record UserUpdateRow(
            string Code,
            string Name,
            bool Active,
            string GroupCode,
            string Mobile,
            DateTime? Dob,
            string? Address,
            string? Nic,
            string? Email,
            string Uid,
            string? Gender,
            bool MemberStatus,
            bool SubscriptionStatus,
            string? SubscriptionType,
            DateTime? ExpireDate,
            int MaxBorrow
        );


        // -------------------- SEARCH USERS --------------------
        public async Task<List<UserSearchRow>> SearchUsersAsync(string query)
        {
            const string sql = @"
SELECT TOP (50) U_CODE, U_NAME
FROM dbo.M_TBLUSERS
WHERE U_CODE LIKE @Q OR U_NAME LIKE @Q
ORDER BY U_NAME;";

            var list = new List<UserSearchRow>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@Q", SqlDbType.NVarChar, 200).Value = "%" + (query ?? "").Trim() + "%";

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(new UserSearchRow(r.GetString(0), r.GetString(1)));

            return list;
        }

        // -------------------- GET USER DETAILS --------------------
        public async Task<UserDetailsRow?> GetUserDetailsAsync(string userCode)
        {
            const string sql = @"
SELECT
    U_CODE, U_NAME, ISNULL(U_ACTIVE,0), U_GROUP, U_MOBILE,
    U_DOB, U_ADDRESS, U_NIC, U_EMAIL,
    ISNULL(NULLIF(U_UID,''), U_CODE) AS U_UID,
    U_GENDER, ISNULL(U_MEMSTATUS,0), ISNULL(U_SUBSSTATUS,0),
    U_SUBSTYPE, U_EXPIREDDATE, ISNULL(U_MAXBORROW,0)
FROM dbo.M_TBLUSERS
WHERE U_CODE = @U;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();

            if (!await r.ReadAsync())
                return null;

            return new UserDetailsRow(
                Code: r.GetString(0),
                Name: r.GetString(1),
                Active: r.GetBoolean(2),
                GroupCode: r.GetString(3),
                Mobile: r.GetString(4),
                Dob: r.IsDBNull(5) ? null : r.GetDateTime(5),
                Address: r.IsDBNull(6) ? null : r.GetString(6),
                Nic: r.IsDBNull(7) ? null : r.GetString(7),
                Email: r.IsDBNull(8) ? null : r.GetString(8),
                Uid: r.GetString(9),
                Gender: r.IsDBNull(10) ? null : r.GetString(10),
                MemberStatus: r.GetBoolean(11),
                SubscriptionStatus: r.GetBoolean(12),
                SubscriptionType: r.IsDBNull(13) ? null : r.GetString(13),
                ExpireDate: r.IsDBNull(14) ? null : r.GetDateTime(14),
                MaxBorrow: r.IsDBNull(15) ? 0 : Convert.ToInt32(r.GetValue(15))
            );
        }

        // -------------------- LOCATIONS (READ) --------------------
        public async Task<List<string>> GetUserLocationCodesAsync(string userCode)
        {
            const string sql = @"
SELECT UL_USERLOC
FROM dbo.M_TBLUSERLOCATION
WHERE UL_USERCODE = @U AND ISNULL(UL_ACTIVE,0)=1;";

            var list = new List<string>();

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(r.GetString(0));

            return list;
        }

        public async Task<int> GetActiveLocationCountAsync()
        {
            const string sql = @"SELECT COUNT(1) FROM dbo.M_LOCATION WHERE L_ACTIVE = 1;";
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            await con.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        // -------------------- UPDATE USER --------------------
        public async Task UpdateUserAsync(SqlConnection con, SqlTransaction tx, UserUpdateRow u)
        {
            const string sql = @"
UPDATE dbo.M_TBLUSERS
SET
    U_NAME       = @U_NAME,
    U_ACTIVE     = @U_ACTIVE,
    U_GROUP      = @U_GROUP,
    U_MOBILE     = @U_MOBILE,
    U_DOB        = @U_DOB,
    U_ADDRESS    = @U_ADDRESS,
    U_NIC        = @U_NIC,
    U_UID        = @U_UID,
    U_GENDER     = @U_GENDER,
    U_MEMSTATUS  = @U_MEMSTATUS,
    U_SUBSSTATUS = @U_SUBSSTATUS,
    U_EMAIL      = @U_EMAIL,
    U_SUBSTYPE   = @U_SUBSTYPE,
    U_EXPIREDDATE= @U_EXPIREDDATE,
    U_MAXBORROW  = @U_MAXBORROW,
    M_DATE       = SYSDATETIME()
WHERE U_CODE = @U_CODE;";

            await using var cmd = new SqlCommand(sql, con, tx);

            cmd.Parameters.Add("@U_CODE", SqlDbType.VarChar, 20).Value = u.Code;
            cmd.Parameters.Add("@U_NAME", SqlDbType.NVarChar, 120).Value = u.Name;
            cmd.Parameters.Add("@U_ACTIVE", SqlDbType.Bit).Value = u.Active;
            cmd.Parameters.Add("@U_GROUP", SqlDbType.VarChar, 20).Value = u.GroupCode;
            cmd.Parameters.Add("@U_MOBILE", SqlDbType.VarChar, 20).Value = u.Mobile;

            cmd.Parameters.Add("@U_DOB", SqlDbType.Date).Value = (object?)u.Dob ?? DBNull.Value;
            cmd.Parameters.Add("@U_ADDRESS", SqlDbType.NVarChar, 250).Value = (object?)u.Address ?? DBNull.Value;
            cmd.Parameters.Add("@U_NIC", SqlDbType.VarChar, 20).Value = (object?)u.Nic ?? DBNull.Value;

            cmd.Parameters.Add("@U_UID", SqlDbType.VarChar, 20).Value = u.Uid;
            cmd.Parameters.Add("@U_GENDER", SqlDbType.VarChar, 20).Value = (object?)u.Gender ?? DBNull.Value;

            cmd.Parameters.Add("@U_MEMSTATUS", SqlDbType.Bit).Value = u.MemberStatus;
            cmd.Parameters.Add("@U_SUBSSTATUS", SqlDbType.Bit).Value = u.SubscriptionStatus;
            cmd.Parameters.Add("@U_EMAIL", SqlDbType.NVarChar, 120).Value = (object?)u.Email ?? DBNull.Value;

            cmd.Parameters.Add("@U_SUBSTYPE", SqlDbType.NVarChar, 120).Value = (object?)u.SubscriptionType ?? DBNull.Value;
            cmd.Parameters.Add("@U_EXPIREDDATE", SqlDbType.DateTime).Value = (object?)u.ExpireDate ?? DBNull.Value;

            cmd.Parameters.Add("@U_MAXBORROW", SqlDbType.Int).Value = u.MaxBorrow;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteUserLocationsAsync(SqlConnection con, SqlTransaction tx, string userCode)
        {
            const string sql = @"DELETE FROM dbo.M_TBLUSERLOCATION WHERE UL_USERCODE = @U;";
            await using var cmd = new SqlCommand(sql, con, tx);
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateUserPasswordAsync(SqlConnection con, SqlTransaction tx, string userCode, string newPasswordHash)
        {
            const string sql = @"
UPDATE dbo.M_TBLUSERS
SET U_PASSWORD = @PWD, M_DATE = SYSDATETIME()
WHERE U_CODE = @U;";

            await using var cmd = new SqlCommand(sql, con, tx);
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;
            cmd.Parameters.Add("@PWD", SqlDbType.NVarChar, 300).Value = newPasswordHash;
            await cmd.ExecuteNonQueryAsync();
        }
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
