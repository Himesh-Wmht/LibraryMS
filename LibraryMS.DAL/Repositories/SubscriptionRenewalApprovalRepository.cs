using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public sealed class SubscriptionRenewalApprovalRepository
    {
        private readonly SqlDb _db;
        public SubscriptionRenewalApprovalRepository(SqlDb db) => _db = db;

        public async Task<List<SubscriptionRenewalApprovalRowDto>> GetPendingAsync()
        {
            const string sql = @"
                SELECT
                    SUB_ID,
                    SUB_UID,
                    SUB_RENEWAL_DATE,
                    SUB_EXPIRY_DATE,
                    SUB_RENEWAL_STATUS,
                    ISNULL(SUB_PAIDAMNT, 0) AS SUB_PAIDAMNT,
                    ISNULL(SUB_DUEAMT, 0) AS SUB_DUEAMT,
                    ISNULL(SUB_PAYAMT, 0) AS SUB_PAYAMT,
                    ISNULL(M_DATE, GETDATE()) AS M_DATE,
                    CAST(ISNULL(SUB_PROCESS, 0) AS bit) AS IsProcessed,
                    CAST(ISNULL(SUB_REJECTED, 0) AS bit) AS IsRejected,
                    SUB_REMARK
                FROM dbo.T_TBLSUBSCRIPTIONRENEWAL
                WHERE ISNULL(SUB_PROCESS, 0) = 0
                  AND ISNULL(SUB_REJECTED, 0) = 0
                ORDER BY M_DATE DESC, SUB_ID DESC;";

            try
            {
                var list = new List<SubscriptionRenewalApprovalRowDto>();

                await using var con = _db.CreateConnection();
                await using var cmd = new SqlCommand(sql, con);

                await con.OpenAsync();
                await using var r = await cmd.ExecuteReaderAsync();

                while (await r.ReadAsync())
                {
                    list.Add(new SubscriptionRenewalApprovalRowDto
                    {
                        SubId = r.GetInt32(0),
                        UserCode = r.IsDBNull(1) ? string.Empty : r.GetString(1),
                        RenewalDate = r.IsDBNull(2) ? DateTime.MinValue : r.GetDateTime(2),
                        ExpiryDate = r.IsDBNull(3) ? DateTime.MinValue : r.GetDateTime(3),
                        RenewalStatus = r.IsDBNull(4) ? null : r.GetString(4),
                        PaidAmt = r.IsDBNull(5) ? 0m : r.GetDecimal(5),
                        DueAmt = r.IsDBNull(6) ? 0m : r.GetDecimal(6),
                        PayAmt = r.IsDBNull(7) ? 0m : r.GetDecimal(7),
                        MDate = r.IsDBNull(8) ? DateTime.MinValue : r.GetDateTime(8),
                        IsProcessed = r.GetBoolean(9),
                        IsRejected = r.GetBoolean(10),
                        Remark = r.IsDBNull(11) ? null : r.GetString(11)
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw DbExceptionHelper.Wrap("SubscriptionRenewalApprovalRepository.GetPendingAsync", ex);
            }
        }

        public async Task<List<SubscriptionRenewalApprovalRowDto>> GetAllAsync()
        {
            const string sql = @"
                SELECT
                    SUB_ID,
                    SUB_UID,
                    SUB_RENEWAL_DATE,
                    SUB_EXPIRY_DATE,
                    SUB_RENEWAL_STATUS,
                    ISNULL(SUB_PAIDAMNT, 0) AS SUB_PAIDAMNT,
                    ISNULL(SUB_DUEAMT, 0) AS SUB_DUEAMT,
                    ISNULL(SUB_PAYAMT, 0) AS SUB_PAYAMT,
                    ISNULL(M_DATE, GETDATE()) AS M_DATE,
                    CAST(ISNULL(SUB_PROCESS, 0) AS bit) AS IsProcessed,
                    CAST(ISNULL(SUB_REJECTED, 0) AS bit) AS IsRejected,
                    SUB_REMARK
                FROM dbo.T_TBLSUBSCRIPTIONRENEWAL
                ORDER BY M_DATE DESC, SUB_ID DESC;";

            try
            {
                var list = new List<SubscriptionRenewalApprovalRowDto>();

                await using var con = _db.CreateConnection();
                await using var cmd = new SqlCommand(sql, con);

                await con.OpenAsync();
                await using var r = await cmd.ExecuteReaderAsync();

                while (await r.ReadAsync())
                {
                    list.Add(new SubscriptionRenewalApprovalRowDto
                    {
                        SubId = r.GetInt32(0),
                        UserCode = r.IsDBNull(1) ? string.Empty : r.GetString(1),
                        RenewalDate = r.IsDBNull(2) ? DateTime.MinValue : r.GetDateTime(2),
                        ExpiryDate = r.IsDBNull(3) ? DateTime.MinValue : r.GetDateTime(3),
                        RenewalStatus = r.IsDBNull(4) ? null : r.GetString(4),
                        PaidAmt = r.IsDBNull(5) ? 0m : r.GetDecimal(5),
                        DueAmt = r.IsDBNull(6) ? 0m : r.GetDecimal(6),
                        PayAmt = r.IsDBNull(7) ? 0m : r.GetDecimal(7),
                        MDate = r.IsDBNull(8) ? DateTime.MinValue : r.GetDateTime(8),
                        IsProcessed = r.GetBoolean(9),
                        IsRejected = r.GetBoolean(10),
                        Remark = r.IsDBNull(11) ? null : r.GetString(11)
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw DbExceptionHelper.Wrap("SubscriptionRenewalApprovalRepository.GetAllAsync", ex);
            }
        }

        public async Task<(bool ok, string message)> ApproveAsync(int subId, string? remark = null, string? newUid = null)
        {
            const string sqlUpdateRenewal = @"
                UPDATE dbo.T_TBLSUBSCRIPTIONRENEWAL
                SET SUB_PROCESS = 1,
                    SUB_REJECTED = 0,
                    SUB_RENEWAL_STATUS = 'Approved',
                    SUB_REMARK = @Remark,
                    M_DATE = GETDATE()
                WHERE SUB_ID = @SubId
                  AND ISNULL(SUB_PROCESS, 0) = 0
                  AND ISNULL(SUB_REJECTED, 0) = 0;";

            const string sqlUpdateUser = @"
                UPDATE u
                SET u.M_DATE = GETDATE(),
                    u.U_UID = CASE
                                WHEN NULLIF(@NewUid, '') IS NULL THEN u.U_UID
                                ELSE @NewUid
                              END,
                    u.U_SUBSSTATUS = 1,
                    u.U_SUBSTYPE = '00003',
                    u.U_EXPIREDDATE = DATEADD(YEAR, 1, GETDATE()),
                    u.U_LOCKED = 0
                FROM dbo.M_TBLUSERS u
                INNER JOIN dbo.T_TBLSUBSCRIPTIONRENEWAL r
                    ON u.U_CODE = r.SUB_UID
                WHERE r.SUB_ID = @SubId;";

            try
            {
                await using var con = _db.CreateConnection();
                await con.OpenAsync();
                await using var tx = (SqlTransaction)await con.BeginTransactionAsync();

                int affectedRenewal;
                await using (var cmd1 = new SqlCommand(sqlUpdateRenewal, con, tx))
                {
                    cmd1.Parameters.Add("@SubId", SqlDbType.Int).Value = subId;
                    cmd1.Parameters.Add("@Remark", SqlDbType.NVarChar, -1).Value = (object?)remark ?? DBNull.Value;
                    affectedRenewal = await cmd1.ExecuteNonQueryAsync();
                }

                if (affectedRenewal == 0)
                {
                    await tx.RollbackAsync();
                    return (false, "Approval failed: already processed / rejected or not found.");
                }

                int affectedUser;
                await using (var cmd2 = new SqlCommand(sqlUpdateUser, con, tx))
                {
                    cmd2.Parameters.Add("@SubId", SqlDbType.Int).Value = subId;
                    cmd2.Parameters.Add("@NewUid", SqlDbType.VarChar, 20).Value = (object?)newUid ?? DBNull.Value;
                    affectedUser = await cmd2.ExecuteNonQueryAsync();
                }

                if (affectedUser == 0)
                {
                    await tx.RollbackAsync();
                    return (false, "Approval failed: matching user was not found.");
                }

                await tx.CommitAsync();
                return (true, "Subscription renewal approved successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Approve failed: {ex.Message}");
            }
        }

        public async Task<(bool ok, string message)> RejectAsync(int subId, string? remark = null)
        {
            const string sql = @"
                UPDATE dbo.T_TBLSUBSCRIPTIONRENEWAL
                SET SUB_PROCESS = 1,
                    SUB_REJECTED = 1,
                    SUB_RENEWAL_STATUS = 'Rejected',
                    SUB_REMARK = @Remark,
                    M_DATE = GETDATE()
                WHERE SUB_ID = @SubId
                  AND ISNULL(SUB_PROCESS, 0) = 0
                  AND ISNULL(SUB_REJECTED, 0) = 0;";

            try
            {
                await using var con = _db.CreateConnection();
                await using var cmd = new SqlCommand(sql, con);

                cmd.Parameters.Add("@SubId", SqlDbType.Int).Value = subId;
                cmd.Parameters.Add("@Remark", SqlDbType.NVarChar, -1).Value = (object?)remark ?? DBNull.Value;

                await con.OpenAsync();
                var affected = await cmd.ExecuteNonQueryAsync();

                if (affected == 0)
                    return (false, "Reject failed: already processed / rejected or not found.");

                return (true, "Subscription renewal rejected successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Reject failed: {ex.Message}");
            }
        }
    }
}