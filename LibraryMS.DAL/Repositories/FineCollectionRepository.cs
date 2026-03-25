using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public sealed class FineCollectionRepository
    {
        private readonly SqlDb _db;
        public FineCollectionRepository(SqlDb db) => _db = db;
        public static class FineStatuses
        {
            public const string Pending = "P";
            public const string PartiallyPaid = "T";
            public const string Paid = "D";
            public const string Waived = "W";
            public const string Refunded = "R";
            public const string Cancelled = "X";
        }
        public async Task<List<MemberFineRowDto>> SearchOpenAsync(string? text)
        {
            const string sql = @"
            SELECT FH_DOCNO, FH_MEMBERCODE, FH_REF_TYPE, FH_REF_DOCNO, FH_FINE_DATE, FH_TOTAL, FH_PAID, FH_BALANCE, FH_STATUS, FH_REMARK
            FROM dbo.T_TBLMEMBERFINE_H
            WHERE FH_BALANCE > 0
              AND (
                    @T IS NULL
                    OR FH_DOCNO LIKE '%' + @T + '%'
                    OR FH_MEMBERCODE LIKE '%' + @T + '%'
                    OR FH_REF_DOCNO LIKE '%' + @T + '%'
                  )
            ORDER BY FH_FINE_DATE DESC;";

            var list = new List<MemberFineRowDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@T", SqlDbType.NVarChar, 200).Value = (object?)NullIfEmpty(text) ?? DBNull.Value;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new MemberFineRowDto(
                    FineDocNo: r.GetString(0),
                    MemberCode: r.GetString(1),
                    RefType: r.GetString(2),
                    RefDocNo: r.GetString(3),
                    FineDate: r.GetDateTime(4),
                    Total: r.GetDecimal(5),
                    Paid: r.GetDecimal(6),
                    Balance: r.GetDecimal(7),
                    Status: r.GetString(8),
                    Remark: r.IsDBNull(9) ? null : r.GetString(9)
                ));
            }
            return list;
        }
        public async Task<MemberFineRowDto?> GetByDocNoAsync(string fineDocNo)
        {
            const string sql = @"
SELECT FH_DOCNO, FH_MEMBERCODE, FH_REF_TYPE, FH_REF_DOCNO, FH_FINE_DATE, FH_TOTAL, FH_PAID, FH_BALANCE, FH_STATUS, FH_REMARK
FROM dbo.T_TBLMEMBERFINE_H
WHERE FH_DOCNO = @D;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = fineDocNo;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();

            if (await r.ReadAsync())
            {
                return new MemberFineRowDto(
                    FineDocNo: r.GetString(0),
                    MemberCode: r.GetString(1),
                    RefType: r.GetString(2),
                    RefDocNo: r.GetString(3),
                    FineDate: r.GetDateTime(4),
                    Total: r.GetDecimal(5),
                    Paid: r.GetDecimal(6),
                    Balance: r.GetDecimal(7),
                    Status: r.GetString(8),
                    Remark: r.IsDBNull(9) ? null : r.GetString(9)
                );
            }

            return null;
        }
        public async Task PayAsync(FinePaymentDto dto)
        {
            await using var con = _db.CreateConnection();
            await con.OpenAsync();
            await using var tx = con.BeginTransaction();

            try
            {
                await using (var cmdP = new SqlCommand(@"
                INSERT INTO dbo.T_TBLMEMBERFINE_PAYMENTS
                (FP_FINE_DOCNO, FP_PAY_DATE, FP_PAY_MODE, FP_AMOUNT, FP_REFNO, FP_STATUS, FP_RECEIVED_BY, FP_REMARK, M_DATE)
                VALUES
                (@D, @PD, @M, @A, @R, 'P', @U, @RM, SYSDATETIME());", con, tx))
                {
                    cmdP.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = dto.FineDocNo;
                    cmdP.Parameters.Add("@PD", SqlDbType.DateTime).Value = dto.PayDate;
                    cmdP.Parameters.Add("@M", SqlDbType.VarChar, 20).Value = dto.PayMode.Trim().ToUpperInvariant();
                    cmdP.Parameters.Add("@A", SqlDbType.Decimal).Value = dto.Amount;
                    cmdP.Parameters.Add("@R", SqlDbType.VarChar, 50).Value = (object?)dto.RefNo ?? DBNull.Value;
                    cmdP.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = dto.ReceivedBy;
                    cmdP.Parameters.Add("@RM", SqlDbType.NVarChar, 200).Value = (object?)dto.Remark ?? DBNull.Value;
                    await cmdP.ExecuteNonQueryAsync();
                }

                await using (var cmdU = new SqlCommand(@"
                UPDATE dbo.T_TBLMEMBERFINE_H
                SET FH_PAID = ISNULL(FH_PAID,0) + @A,
                    FH_BALANCE = FH_TOTAL - (ISNULL(FH_PAID,0) + @A),
                    FH_STATUS =
                        CASE
                            WHEN FH_TOTAL - (ISNULL(FH_PAID,0) + @A) <= 0 THEN @PD
                            ELSE @PT
                        END,
                    M_DATE = SYSDATETIME()
                WHERE FH_DOCNO = @D;", con, tx))
                {
                    cmdU.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = dto.FineDocNo;
                    cmdU.Parameters.Add("@A", SqlDbType.Decimal).Value = dto.Amount;
                    cmdU.Parameters.Add("@PD", SqlDbType.VarChar, 1).Value = FineStatuses.Paid;
                    cmdU.Parameters.Add("@PT", SqlDbType.VarChar, 1).Value = FineStatuses.PartiallyPaid;
                    await cmdU.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
        public async Task<List<FineDetailRowDto>> GetDetailsAsync(string fineDocNo)
        {
            const string sql = @"
                SELECT FD_FINE_TYPE, FD_BOOKCODE, FD_QTY, FD_RATE, FD_AMOUNT, FD_REMARK
                FROM dbo.T_TBLMEMBERFINE_D
                WHERE FD_DOCNO = @D
                ORDER BY FD_LINENO;";

            var list = new List<FineDetailRowDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = fineDocNo;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new FineDetailRowDto(
                    FineType: r.GetString(0),
                    BookCode: r.IsDBNull(1) ? null : r.GetString(1),
                    Qty: r.GetInt32(2),
                    Rate: r.GetDecimal(3),
                    Amount: r.GetDecimal(4),
                    Remark: r.IsDBNull(5) ? null : r.GetString(5)
                ));
            }

            return list;
        }
        public async Task RefundAsync(FineRefundDto dto)
        {
            await using var con = _db.CreateConnection();
            await con.OpenAsync();
            await using var tx = con.BeginTransaction();

            try
            {
                await using (var cmdR = new SqlCommand(@"
                INSERT INTO dbo.T_TBLMEMBERFINE_REFUNDS
                (FR_FINE_DOCNO, FR_REFUND_DATE, FR_AMOUNT, FR_MODE, FR_REASON, FR_STATUS, FR_APPROVED_BY, FR_REMARK, M_DATE)
                VALUES
                (@D, @RD, @A, @M, @RS, 'P', @U, @RM, SYSDATETIME());", con, tx))
                {
                    cmdR.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = dto.FineDocNo;
                    cmdR.Parameters.Add("@RD", SqlDbType.DateTime).Value = dto.RefundDate;
                    cmdR.Parameters.Add("@A", SqlDbType.Decimal).Value = dto.Amount;
                    cmdR.Parameters.Add("@M", SqlDbType.VarChar, 20).Value = dto.Mode.Trim().ToUpperInvariant();
                    cmdR.Parameters.Add("@RS", SqlDbType.VarChar, 20).Value = dto.Reason.Trim().ToUpperInvariant();
                    cmdR.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = dto.ApprovedBy;
                    cmdR.Parameters.Add("@RM", SqlDbType.NVarChar, 200).Value = (object?)dto.Remark ?? DBNull.Value;
                    await cmdR.ExecuteNonQueryAsync();
                }

                await using (var cmdU = new SqlCommand(@"
                UPDATE dbo.T_TBLMEMBERFINE_H
                SET FH_STATUS = @S,
                    M_DATE = SYSDATETIME()
                WHERE FH_DOCNO = @D;", con, tx))
                {
                    cmdU.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = dto.FineDocNo;
                    cmdU.Parameters.Add("@S", SqlDbType.VarChar, 1).Value = FineStatuses.Refunded;
                    await cmdU.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}