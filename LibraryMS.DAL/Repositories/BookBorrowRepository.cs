using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Transactions;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public sealed class BookBorrowRepository
    {
        private readonly SqlDb _db;
        public BookBorrowRepository(SqlDb db) => _db = db;
        public static class BorrowStatuses
        {
            public const string Open = "O";
            public const string PartialReturn = "P";
            public const string Closed = "C";
            public const string Cancelled = "X";
            public const string LostClosed = "L";
        }
        public static class ReservationStatuses
        {
            public const string Pending = "P";
            public const string Active = "A";
            public const string Rejected = "R";
            public const string Cancelled = "C";
            public const string Fulfilled = "F";
            public const string Expired = "E";
        }

        public async Task<string> CreateAsync(BorrowCreateDto dto)
        {
            var docNo = "BRW" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            await using var con = _db.CreateConnection();
            await con.OpenAsync();
            await using var tx = con.BeginTransaction();

            try
            {
                await using (var cmdH = new SqlCommand(@"
INSERT INTO dbo.T_TBLBOOKBORROW_H
(BH_DOCNO, BH_MEMBERCODE, BH_LOCCODE, BH_BORROWDATE, BH_STATUS, BH_REMARK, BH_CREATED_BY, BH_CREATED_DATE, M_DATE)
VALUES
(@D, @M, @L, SYSDATETIME(), @S, @R, @U, SYSDATETIME(), SYSDATETIME());", con, tx))
                {
                    cmdH.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = docNo;
                    cmdH.Parameters.Add("@M", SqlDbType.VarChar, 20).Value = dto.MemberCode;
                    cmdH.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = dto.LocCode;
                    cmdH.Parameters.Add("@S", SqlDbType.VarChar, 1).Value = BorrowStatuses.Open;
                    cmdH.Parameters.Add("@R", SqlDbType.NVarChar, 200).Value = (object?)dto.Remark ?? DBNull.Value;
                    cmdH.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = dto.BorrowedBy;
                    await cmdH.ExecuteNonQueryAsync();
                }

                var lineNo = 1;
                foreach (var line in dto.Lines)
                {
                    await ValidateAvailableQtyAsync(con, tx, dto.LocCode, line.BookCode, line.Qty, line.ReservationId);

                    await using (var cmdD = new SqlCommand(@"
INSERT INTO dbo.T_TBLBOOKBORROW_D
(BD_DOCNO, BD_LINENO, BD_BOOKCODE, BD_QTY, BD_RETURNED_QTY, BD_DUEDATE, BD_STATUS, BD_REMARK, BD_RESERVATION_ID, M_DATE)
VALUES
(@D, @L, @B, @Q, 0, @DU, @S, NULL, @RID, SYSDATETIME());", con, tx))
                    {
                        cmdD.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = docNo;
                        cmdD.Parameters.Add("@L", SqlDbType.Int).Value = lineNo;
                        cmdD.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = line.BookCode;
                        cmdD.Parameters.Add("@Q", SqlDbType.Int).Value = line.Qty;
                        cmdD.Parameters.Add("@DU", SqlDbType.Date).Value = line.DueDate.Date;
                        cmdD.Parameters.Add("@S", SqlDbType.VarChar, 1).Value = BorrowStatuses.Open;
                        cmdD.Parameters.Add("@RID", SqlDbType.Int).Value = (object?)line.ReservationId ?? DBNull.Value;
                        await cmdD.ExecuteNonQueryAsync();
                    }

                    await using (var cmdInv = new SqlCommand(@"
UPDATE dbo.T_TBLBOOKINVENTORY
SET BI_QTY = ISNULL(BI_QTY,0) - @Q,
    M_DATE = SYSDATETIME()
WHERE BI_LOCCODE = @L
  AND BI_BOOKCODE = @B
  AND ISNULL(BI_ACTIVE,0)=1;", con, tx))
                    {
                        cmdInv.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = dto.LocCode;
                        cmdInv.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = line.BookCode;
                        cmdInv.Parameters.Add("@Q", SqlDbType.Int).Value = line.Qty;
                        await cmdInv.ExecuteNonQueryAsync();
                    }

                    if (line.ReservationId.HasValue)
                    {
                        await using var cmdRes = new SqlCommand(@"
UPDATE dbo.T_TBLBOOKRESERVATIONS
SET BR_STATUS = @S,
    BR_PROC_BY = @U,
    BR_PROC_AT = SYSDATETIME(),
    BR_BORROW_DOCNO = @D,
    BR_BORROW_LINENO = @LN,
    M_DATE = SYSDATETIME()
WHERE BR_ID = @RID
  AND BR_STATUS = 'A';", con, tx);

                        cmdRes.Parameters.Add("@S", SqlDbType.VarChar, 1).Value = ReservationStatuses.Fulfilled;
                        cmdRes.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = dto.BorrowedBy;
                        cmdRes.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = docNo;
                        cmdRes.Parameters.Add("@LN", SqlDbType.Int).Value = lineNo;
                        cmdRes.Parameters.Add("@RID", SqlDbType.Int).Value = line.ReservationId.Value;
                        await cmdRes.ExecuteNonQueryAsync();
                    }

                    lineNo++;
                }

                await tx.CommitAsync();
                return docNo;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<List<BorrowOpenRowDto>> SearchOpenAsync(string locCode, string? text)
        {
            const string sql = @"
SELECT h.BH_DOCNO,
       h.BH_MEMBERCODE,
       ISNULL(u.U_NAME,'') AS U_NAME,
       h.BH_LOCCODE,
       h.BH_BORROWDATE,
       h.BH_STATUS
FROM dbo.T_TBLBOOKBORROW_H h
LEFT JOIN dbo.M_TBLUSERS u ON u.U_CODE = h.BH_MEMBERCODE
WHERE h.BH_LOCCODE = @L
  AND h.BH_STATUS IN ('O','P')
  AND (
        @T IS NULL
        OR h.BH_DOCNO LIKE '%' + @T + '%'
        OR h.BH_MEMBERCODE LIKE '%' + @T + '%'
        OR ISNULL(u.U_NAME,'') LIKE '%' + @T + '%'
      )
ORDER BY h.BH_BORROWDATE DESC;";

            var list = new List<BorrowOpenRowDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = locCode;
            cmd.Parameters.Add("@T", SqlDbType.NVarChar, 200).Value = (object?)NullIfEmpty(text) ?? DBNull.Value;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new BorrowOpenRowDto(
                    DocNo: r.GetString(0),
                    MemberCode: r.GetString(1),
                    MemberName: r.GetString(2),
                    LocCode: r.GetString(3),
                    BorrowDate: r.GetDateTime(4),
                    Status: r.GetString(5)
                ));
            }
            return list;
        }

        public async Task<List<BorrowOpenDetailRowDto>> GetOpenDetailsAsync(string docNo)
        {
            const string sql = @"
SELECT d.BD_LINENO,
       d.BD_BOOKCODE,
       b.B_TITLE,
       d.BD_QTY,
       ISNULL(d.BD_RETURNED_QTY,0),
       d.BD_QTY - ISNULL(d.BD_RETURNED_QTY,0) AS OutstandingQty,
       d.BD_DUEDATE,
       ISNULL(b.B_PRICE,0),
       d.BD_RESERVATION_ID
FROM dbo.T_TBLBOOKBORROW_D d
JOIN dbo.M_TBLBOOKS b ON b.B_CODE = d.BD_BOOKCODE
WHERE d.BD_DOCNO = @D
  AND d.BD_QTY - ISNULL(d.BD_RETURNED_QTY,0) > 0
ORDER BY d.BD_LINENO;";

            var list = new List<BorrowOpenDetailRowDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = docNo;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new BorrowOpenDetailRowDto(
                    LineNo: r.GetInt32(0),
                    BookCode: r.GetString(1),
                    Title: r.GetString(2),
                    Qty: r.GetInt32(3),
                    ReturnedQty: r.GetInt32(4),
                    OutstandingQty: r.GetInt32(5),
                    DueDate: r.GetDateTime(6),
                    ReplacementCost: r.GetDecimal(7),
                    ReservationId: r.IsDBNull(8) ? null : r.GetInt32(8)
                ));
            }
            return list;
        }

        private static async Task ValidateAvailableQtyAsync(SqlConnection con, SqlTransaction tx, string locCode, string bookCode, int reqQty, int? reservationId)
        {
            const string sql = @"
WITH R AS
(
    SELECT
        BR_BOOKCODE,
        BR_LOCCODE,
        SUM(BR_QTY) AS ReservedQty
    FROM dbo.T_TBLBOOKRESERVATIONS
    WHERE BR_STATUS = 'A'
      AND BR_EXPIRES_ON >= CONVERT(date, SYSDATETIME())
      AND (@RID IS NULL OR BR_ID <> @RID)
    GROUP BY BR_BOOKCODE, BR_LOCCODE
)
SELECT
    CASE
        WHEN ISNULL(i.BI_QTY,0) - ISNULL(r.ReservedQty,0) < 0 THEN 0
        ELSE ISNULL(i.BI_QTY,0) - ISNULL(r.ReservedQty,0)
    END
FROM dbo.T_TBLBOOKINVENTORY i
LEFT JOIN R r ON r.BR_BOOKCODE = i.BI_BOOKCODE AND r.BR_LOCCODE = i.BI_LOCCODE
WHERE i.BI_LOCCODE = @L
  AND i.BI_BOOKCODE = @B
  AND ISNULL(i.BI_ACTIVE,0)=1;";

            await using var cmd = new SqlCommand(sql, con, tx);
            cmd.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = locCode;
            cmd.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = bookCode;
            cmd.Parameters.Add("@RID", SqlDbType.Int).Value = (object?)reservationId ?? DBNull.Value;

            var obj = await cmd.ExecuteScalarAsync();
            var available = obj == null || obj == DBNull.Value ? 0 : Convert.ToInt32(obj);

            if (available < reqQty)
                throw new InvalidOperationException($"Insufficient available qty for book {bookCode}. Available: {available}, Requested: {reqQty}");
        }

        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}