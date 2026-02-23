using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public sealed class ApprovalRepository
    {
        private readonly SqlDb _db;
        public ApprovalRepository(SqlDb db) => _db = db;

        public async Task<List<ApprovalRowDto>> GetPendingAsync()
        {
            const string sql = @"
                            SELECT
                                AP_ID, AP_U_ID, AP_U_UID, AP_NAME, AP_MOBILE, AP_GROUP, AP_SUBSTYPE,
                                AP_PAIDAMT, AP_DUEAMT, AP_PAYMENTMETHOD, AP_REFERENCENO, AP_APPROVEDBY, AP_DATE,
                                CAST(CASE WHEN ISNULL(AP_PROCCESS,0)=1 AND ISNULL(AP_CALCEL,0)=0 THEN 1 ELSE 0 END AS bit) AS IsApproved,
                                CAST(CASE WHEN ISNULL(AP_PROCCESS,0)=1 AND ISNULL(AP_CALCEL,0)=1 THEN 1 ELSE 0 END AS bit) AS IsRejected
                            FROM dbo.T_TBLAPPROVAL WHERE ISNULL(AP_PROCCESS,0)=0 AND ISNULL(AP_CALCEL,0)=0 ORDER BY AP_DATE DESC;";

            try
            {
                var list = new List<ApprovalRowDto>();

                await using var con = _db.CreateConnection();
                await using var cmd = new SqlCommand(sql, con);

                await con.OpenAsync();
                await using var r = await cmd.ExecuteReaderAsync();

                while (await r.ReadAsync())
                {
                    list.Add(new ApprovalRowDto(
                        ApId: r.GetString(0).Trim(),
                        UserCode: r.GetString(1),
                        UserUid: r.IsDBNull(2) ? null : r.GetString(2),
                        Name: r.GetString(3),
                        Mobile: r.IsDBNull(4) ? null : r.GetString(4),
                        GroupCode: r.GetString(5),
                        SubType: r.IsDBNull(6) ? null : r.GetString(6),
                        PaidAmt: r.IsDBNull(7) ? 0m : r.GetDecimal(7),
                        DueAmt: r.IsDBNull(8) ? 0m : r.GetDecimal(8),
                        PaymentMethod: r.IsDBNull(9) ? null : r.GetString(9),
                        ReferenceNo: r.IsDBNull(10) ? null : r.GetString(10),
                        ApprovedBy: r.IsDBNull(11) ? null : r.GetString(11),
                        ApDate: r.IsDBNull(12) ? DateTime.MinValue : r.GetDateTime(12),
                        IsApproved: r.GetBoolean(13), 
                        IsRejected: r.GetBoolean(14)
                    ));
                }

                return list;
            }
            catch (Exception ex)
            {
                throw DbExceptionHelper.Wrap("ApprovalRepository.GetPendingAsync", ex);
            }
        }
        public async Task<List<ApprovalRowDto>> GetAllAsync()
        {
            const string sql = @"
                    SELECT
                        AP_ID, AP_U_ID, AP_U_UID, AP_NAME, AP_MOBILE, AP_GROUP, AP_SUBSTYPE,
                        AP_PAIDAMT, AP_DUEAMT, AP_PAYMENTMETHOD, AP_REFERENCENO, AP_APPROVEDBY, AP_DATE,
                        CAST(CASE WHEN ISNULL(AP_PROCCESS,0)=1 AND ISNULL(AP_CALCEL,0)=0 THEN 1 ELSE 0 END AS bit) AS IsApproved,
                        CAST(CASE WHEN ISNULL(AP_PROCCESS,0)=1 AND ISNULL(AP_CALCEL,0)=1 THEN 1 ELSE 0 END AS bit) AS IsRejected
                    FROM dbo.T_TBLAPPROVAL ORDER BY AP_DATE DESC;";
            try
            {
                var list = new List<ApprovalRowDto>();

                await using var con = _db.CreateConnection();
                await using var cmd = new SqlCommand(sql, con);

                await con.OpenAsync();
                await using var r = await cmd.ExecuteReaderAsync();

                while (await r.ReadAsync())
                {
                    list.Add(new ApprovalRowDto(
                        ApId: r.GetString(0).Trim(),
                        UserCode: r.GetString(1),
                        UserUid: r.IsDBNull(2) ? null : r.GetString(2),
                        Name: r.GetString(3),
                        Mobile: r.IsDBNull(4) ? null : r.GetString(4),
                        GroupCode: r.GetString(5),
                        SubType: r.IsDBNull(6) ? null : r.GetString(6),
                        PaidAmt: r.IsDBNull(7) ? 0m : r.GetDecimal(7),
                        DueAmt: r.IsDBNull(8) ? 0m : r.GetDecimal(8),
                        PaymentMethod: r.IsDBNull(9) ? null : r.GetString(9),
                        ReferenceNo: r.IsDBNull(10) ? null : r.GetString(10),
                        ApprovedBy: r.IsDBNull(11) ? null : r.GetString(11),
                        ApDate: r.IsDBNull(12) ? DateTime.MinValue : r.GetDateTime(12),
                        IsApproved: r.GetBoolean(13),
                        IsRejected: r.GetBoolean(14)
                    ));
                }

                return list;
            }
            catch (Exception ex)
            {
                throw DbExceptionHelper.Wrap("ApprovalRepository.GetAllAsync", ex);
            }
        }

        public async Task<string> InsertPendingAsync(SqlConnection con, SqlTransaction tx, ApprovalInsertDto dto)
        {
            const string sqlNextId = @"
                            SELECT 'AP' + RIGHT('00000000' + CAST(
                                ISNULL(MAX(TRY_CAST(SUBSTRING(AP_ID,3,8) AS INT)),0) + 1 AS VARCHAR(8)), 8)
                            FROM dbo.T_TBLAPPROVAL WITH (UPDLOCK, HOLDLOCK)
                            WHERE AP_ID LIKE 'AP[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]';";

            const string sqlInsert = @"
                        INSERT INTO dbo.T_TBLAPPROVAL (AP_ID, AP_U_ID, AP_U_UID, AP_NAME, AP_MOBILE, AP_GROUP, AP_SUBSTYPE,
                         AP_PAIDAMT, AP_DUEAMT, AP_PAYMENTMETHOD, AP_REFERENCENO, AP_APPROVEDBY, AP_DATE, AP_PROCCESS, AP_CALCEL)
                        VALUES (@AP_ID, @AP_U_ID, @AP_U_UID, @AP_NAME, @AP_MOBILE, @AP_GROUP, @AP_SUBSTYPE, @AP_PAIDAMT, @AP_DUEAMT, @AP_PAYMENTMETHOD, @AP_REFERENCENO, @AP_APPROVEDBY, @AP_DATE, @AP_PROCCESS, @AP_CALCEL);";


            try
            {
                string apId;
                await using (var cmdId = new SqlCommand(sqlNextId, con, tx))
                    apId = (string)(await cmdId.ExecuteScalarAsync() ?? "AP00000001");

                await using (var cmd = new SqlCommand(sqlInsert, con, tx))
                {
                    cmd.Parameters.Add("@AP_ID", SqlDbType.NChar, 10).Value = apId;
                    cmd.Parameters.Add("@AP_U_ID", SqlDbType.NVarChar, 100).Value = dto.UserCode;
                    cmd.Parameters.Add("@AP_U_UID", SqlDbType.NVarChar, 100).Value = (object?)dto.UserUid ?? DBNull.Value;
                    cmd.Parameters.Add("@AP_NAME", SqlDbType.NVarChar, 100).Value = dto.Name;
                    cmd.Parameters.Add("@AP_MOBILE", SqlDbType.NVarChar, 100).Value = (object?)dto.Mobile ?? DBNull.Value;
                    cmd.Parameters.Add("@AP_GROUP", SqlDbType.NVarChar, 100).Value = dto.GroupCode;
                    cmd.Parameters.Add("@AP_SUBSTYPE", SqlDbType.NVarChar, 100).Value = (object?)dto.SubType ?? DBNull.Value;

                    var pPaid = cmd.Parameters.Add("@AP_PAIDAMT", SqlDbType.Decimal);
                    pPaid.Precision = 18; pPaid.Scale = 2; pPaid.Value = dto.PaidAmt;

                    var pDue = cmd.Parameters.Add("@AP_DUEAMT", SqlDbType.Decimal);
                    pDue.Precision = 18; pDue.Scale = 2; pDue.Value = dto.DueAmt;

                    cmd.Parameters.Add("@AP_PAYMENTMETHOD", SqlDbType.NVarChar, 100).Value = (object?)dto.PaymentMethod ?? DBNull.Value;
                    cmd.Parameters.Add("@AP_REFERENCENO", SqlDbType.NVarChar, 100).Value = (object?)dto.ReferenceNo ?? DBNull.Value;
                    cmd.Parameters.Add("@AP_APPROVEDBY", SqlDbType.NVarChar, 100).Value =(object?)dto.ApprovedBy ?? DBNull.Value;

                    cmd.Parameters.Add("@AP_DATE", SqlDbType.DateTime).Value =dto.ApDate == default ? DateTime.Now : dto.ApDate;

                    cmd.Parameters.Add("@AP_PROCCESS", SqlDbType.Bit).Value = dto.Processed;
                    cmd.Parameters.Add("@AP_CALCEL", SqlDbType.Bit).Value = dto.Canceled;
                    await cmd.ExecuteNonQueryAsync();
                }

                return apId;
            }
            catch (Exception ex)
            {
                throw DbExceptionHelper.Wrap("ApprovalRepository.InsertPendingAsync(tx)", ex);
            }
        }

        public async Task<(bool ok, string message)> ApproveAsync(string apId, string approvedBy)
        {
            const string sqlUpdateApproval = @"
                    UPDATE dbo.T_TBLAPPROVAL
                    SET AP_PROCCESS = 1,
                        AP_CALCEL = 0,
                        AP_APPROVEDBY = @ApprovedBy,
                        AP_DATE = GETDATE()
                    WHERE AP_ID = @ApId
                      AND ISNULL(AP_PROCCESS,0) = 0
                      AND ISNULL(AP_CALCEL,0) = 0;";

            const string sqlActivateUser = @"
                UPDATE u
                SET u.U_ACTIVE = 1
                FROM dbo.M_TBLUSERS u
                INNER JOIN dbo.T_TBLAPPROVAL a ON a.AP_U_ID = u.U_CODE
                WHERE a.AP_ID = @ApId;";

            try
            {
                await using var con = _db.CreateConnection();
                await con.OpenAsync();
                await using var tx = (SqlTransaction)await con.BeginTransactionAsync();

                int affected;
                await using (var cmd = new SqlCommand(sqlUpdateApproval, (SqlConnection)con, tx))
                {
                    cmd.Parameters.Add("@ApId", SqlDbType.NChar, 10).Value = apId;
                    cmd.Parameters.Add("@ApprovedBy", SqlDbType.NVarChar, 100).Value = approvedBy;
                    affected = await cmd.ExecuteNonQueryAsync();
                }

                if (affected == 0)
                {
                    await tx.RollbackAsync();
                    return (false, "Approval already processed / cancelled, or not found.");
                }

                await using (var cmd2 = new SqlCommand(sqlActivateUser, (SqlConnection)con, tx))
                {
                    cmd2.Parameters.Add("@ApId", SqlDbType.NChar, 10).Value = apId;
                    await cmd2.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
                return (true, "Approved successfully. User activated.");
            }
            catch (Exception ex)
            {
                return (false, $"Approve failed: {ex.Message}");
            }
        }

        public async Task<(bool ok, string message)> RejectAsync(string apId, string rejectedBy)
        {
            const string sql = @"
                        UPDATE dbo.T_TBLAPPROVAL
                        SET AP_PROCCESS = 1,
                            AP_CALCEL = 1,
                            AP_APPROVEDBY = @RejectedBy,
                            AP_DATE = GETDATE()
                        WHERE AP_ID = @ApId
                          AND ISNULL(AP_PROCCESS,0) = 0
                          AND ISNULL(AP_CALCEL,0) = 0;";

            try
            {
                await using var con = _db.CreateConnection();
                await using var cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add("@ApId", SqlDbType.NChar, 10).Value = apId;
                cmd.Parameters.Add("@RejectedBy", SqlDbType.NVarChar, 100).Value = rejectedBy;

                await con.OpenAsync();
                var affected = await cmd.ExecuteNonQueryAsync();

                if (affected == 0)
                    return (false, "Reject failed: already processed / cancelled or not found.");

                return (true, "Rejected successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Reject failed: {ex.Message}");
            }
        }
        public async Task<(bool ok, string message)> SettleDueAsync(string apId, decimal payAmount, string? paymentMethod, string? referenceNo)
        {
            const string sql = @"
                        UPDATE dbo.T_TBLAPPROVAL
                        SET
                            AP_PAIDAMT = ISNULL(AP_PAIDAMT, 0) + @Pay,
                            AP_DUEAMT  = ISNULL(AP_DUEAMT, 0) - @Pay,
                            AP_PAYMENTMETHOD = @Method,
                            AP_REFERENCENO   = @Ref
                        WHERE
                            AP_ID = @ApId

                            AND ISNULL(AP_CALCEL,0) = 0
                            AND ISNULL(AP_DUEAMT,0) >= @Pay;";

            try
            {
                if (payAmount <= 0m)
                    return (false, "Pay amount must be greater than 0.");

                await using var con = _db.CreateConnection();
                await using var cmd = new SqlCommand(sql, con);

                cmd.Parameters.Add("@ApId", SqlDbType.NChar, 10).Value = apId;

                var pPay = cmd.Parameters.Add("@Pay", SqlDbType.Decimal);
                pPay.Precision = 18; pPay.Scale = 2; pPay.Value = payAmount;

                cmd.Parameters.Add("@Method", SqlDbType.NVarChar, 100).Value =
                    (object?)paymentMethod ?? DBNull.Value;

                cmd.Parameters.Add("@Ref", SqlDbType.NVarChar, 100).Value =
                    (object?)referenceNo ?? DBNull.Value;

                await con.OpenAsync();
                var affected = await cmd.ExecuteNonQueryAsync();

                if (affected == 0)
                    return (false, "Settle failed: record not found, already processed/cancelled, or pay amount exceeds due.");

                return (true, "Due amount settled successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Settle failed: {ex.Message}");
            }
        }

    }
}
