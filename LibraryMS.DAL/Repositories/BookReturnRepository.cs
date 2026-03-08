using System.Data;
using System.Data.SqlClient;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public sealed class BookReturnRepository
    {
        private readonly SqlDb _db;
        public BookReturnRepository(SqlDb db) => _db = db;
        public static class FineStatuses
        {
            public const string Pending = "P";
            public const string PartiallyPaid = "T";
            public const string Paid = "D";
            public const string Waived = "W";
            public const string Refunded = "R";
            public const string Cancelled = "X";
        }
        public async Task<BorrowOpenDetailRowDto?> GetBorrowLineAsync(string borrowDocNo, int lineNo)
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
                                  AND d.BD_LINENO = @L;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = borrowDocNo;
            cmd.Parameters.Add("@L", SqlDbType.Int).Value = lineNo;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return null;

            return new BorrowOpenDetailRowDto(
                LineNo: r.GetInt32(0),
                BookCode: r.GetString(1),
                Title: r.GetString(2),
                Qty: r.GetInt32(3),
                ReturnedQty: r.GetInt32(4),
                OutstandingQty: r.GetInt32(5),
                DueDate: r.GetDateTime(6),
                ReplacementCost: r.GetDecimal(7),
                ReservationId: r.IsDBNull(8) ? null : r.GetInt32(8)
            );
        }

        public async Task<ReturnProcessResultDto> CreateAsync(ReturnCreateDto dto, List<FineLineDto> fineLines)
        {
            var returnDocNo = "RTN" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var fineTotal = fineLines.Sum(x => x.Amount);
            var fineDocNo = fineTotal > 0 ? "FIN" + DateTime.Now.ToString("yyyyMMddHHmmssfff") : null;

            await using var con = _db.CreateConnection();
            await con.OpenAsync();
            await using var tx = con.BeginTransaction();

            try
            {
                await using (var cmdH = new SqlCommand(@"
                                                        INSERT INTO dbo.T_TBLBOOKRETURN_H
                                                        (RH_DOCNO, RH_BORROW_DOCNO, RH_MEMBERCODE, RH_LOCCODE, RH_RETURNDATE, RH_STATUS, RH_REMARK, RH_CREATED_BY, RH_CREATED_DATE, M_DATE)
                                                        VALUES
                                                        (@D, @BD, @M, @L, SYSDATETIME(), @S, @R, @U, SYSDATETIME(), SYSDATETIME());", con, tx))
                {
                    cmdH.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = returnDocNo;
                    cmdH.Parameters.Add("@BD", SqlDbType.VarChar, 30).Value = dto.BorrowDocNo;
                    cmdH.Parameters.Add("@M", SqlDbType.VarChar, 20).Value = dto.MemberCode;
                    cmdH.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = dto.LocCode;
                    cmdH.Parameters.Add("@S", SqlDbType.VarChar, 1).Value = ReturnStatuses.Closed;
                    cmdH.Parameters.Add("@R", SqlDbType.NVarChar, 200).Value = (object?)dto.Remark ?? DBNull.Value;
                    cmdH.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = dto.ReturnedBy;
                    await cmdH.ExecuteNonQueryAsync();
                }

                var lineNo = 1;
                foreach (var line in dto.Lines)
                {
                    var fineAmt = fineLines
                        .Where(x => string.Equals(x.BookCode, line.BookCode, StringComparison.OrdinalIgnoreCase))
                        .Sum(x => x.Amount);

                    await using (var cmdD = new SqlCommand(@"
                        INSERT INTO dbo.T_TBLBOOKRETURN_D
                        (RD_DOCNO, RD_LINENO, RD_BORROW_DOCNO, RD_BORROW_LINENO, RD_BOOKCODE, RD_RETURN_QTY, RD_CONDITION, RD_FINE_AMOUNT, RD_REMARK, M_DATE)
                        VALUES
                        (@D, @L, @BD, @BL, @B, @Q, @C, @F, @R, SYSDATETIME());", con, tx))
                    {
                        cmdD.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = returnDocNo;
                        cmdD.Parameters.Add("@L", SqlDbType.Int).Value = lineNo;
                        cmdD.Parameters.Add("@BD", SqlDbType.VarChar, 30).Value = dto.BorrowDocNo;
                        cmdD.Parameters.Add("@BL", SqlDbType.Int).Value = line.BorrowLineNo;
                        cmdD.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = line.BookCode;
                        cmdD.Parameters.Add("@Q", SqlDbType.Int).Value = line.Qty;
                        cmdD.Parameters.Add("@C", SqlDbType.VarChar, 1).Value = Normalize(line.Condition);
                        cmdD.Parameters.Add("@F", SqlDbType.Decimal).Value = fineAmt;
                        cmdD.Parameters.Add("@R", SqlDbType.NVarChar, 200).Value = (object?)line.Remark ?? DBNull.Value;
                        await cmdD.ExecuteNonQueryAsync();
                    }

                    if (Normalize(line.Condition) != ReturnConditions.Lost)
                    {
                        await using var cmdInv = new SqlCommand(@"
                        IF EXISTS (SELECT 1 FROM dbo.T_TBLBOOKINVENTORY WHERE BI_BOOKCODE=@B AND BI_LOCCODE=@L)
                        BEGIN
                            UPDATE dbo.T_TBLBOOKINVENTORY
                            SET BI_QTY = ISNULL(BI_QTY,0) + @Q,
                                BI_ACTIVE = 1,
                                M_DATE = SYSDATETIME()
                            WHERE BI_BOOKCODE=@B AND BI_LOCCODE=@L;
                        END
                        ELSE
                        BEGIN
                            INSERT INTO dbo.T_TBLBOOKINVENTORY
                            (BI_BOOKCODE, BI_LOCCODE, BI_QTY, BI_REORDER, BI_ACTIVE, M_DATE)
                            VALUES
                            (@B, @L, @Q, 0, 1, SYSDATETIME());
                        END", con, tx);

                        cmdInv.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = line.BookCode;
                        cmdInv.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = dto.LocCode;
                        cmdInv.Parameters.Add("@Q", SqlDbType.Int).Value = line.Qty;
                        await cmdInv.ExecuteNonQueryAsync();
                    }

                    await using var cmdUpd = new SqlCommand(@"
                    UPDATE dbo.T_TBLBOOKBORROW_D
                    SET BD_RETURNED_QTY = ISNULL(BD_RETURNED_QTY,0) + @Q,
                        BD_STATUS =
                            CASE
                                WHEN ISNULL(BD_RETURNED_QTY,0) + @Q >= BD_QTY
                                    THEN CASE WHEN @C = 'L' THEN 'L' ELSE 'C' END
                                ELSE 'P'
                            END,
                        M_DATE = SYSDATETIME()
                    WHERE BD_DOCNO=@D AND BD_LINENO=@L;", con, tx);

                    cmdUpd.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = dto.BorrowDocNo;
                    cmdUpd.Parameters.Add("@L", SqlDbType.Int).Value = line.BorrowLineNo;
                    cmdUpd.Parameters.Add("@Q", SqlDbType.Int).Value = line.Qty;
                    cmdUpd.Parameters.Add("@C", SqlDbType.VarChar, 1).Value = Normalize(line.Condition);
                    await cmdUpd.ExecuteNonQueryAsync();

                    lineNo++;
                }

                await using (var cmdHdr = new SqlCommand(@"
                UPDATE dbo.T_TBLBOOKBORROW_H
                SET BH_STATUS =
                    CASE
                        WHEN EXISTS
                        (
                            SELECT 1
                            FROM dbo.T_TBLBOOKBORROW_D
                            WHERE BD_DOCNO=@D
                              AND BD_STATUS IN ('O','P')
                        )
                        THEN 'P'
                        ELSE 'C'
                    END,
                    M_DATE = SYSDATETIME()
                WHERE BH_DOCNO=@D;", con, tx))
                {
                    cmdHdr.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = dto.BorrowDocNo;
                    await cmdHdr.ExecuteNonQueryAsync();
                }

                if (fineTotal > 0 && fineDocNo != null)
                {
                    await using (var cmdFH = new SqlCommand(@"
                    INSERT INTO dbo.T_TBLMEMBERFINE_H
                    (FH_DOCNO, FH_MEMBERCODE, FH_REF_TYPE, FH_REF_DOCNO, FH_FINE_DATE, FH_TOTAL, FH_PAID, FH_BALANCE, FH_STATUS, FH_REMARK, FH_CREATED_BY, FH_CREATED_DATE, M_DATE)
                    VALUES
                    (@D, @M, 'RETURN', @R, SYSDATETIME(), @T, 0, @T, @S, NULL, @U, SYSDATETIME(), SYSDATETIME());", con, tx))
                    {
                        cmdFH.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = fineDocNo;
                        cmdFH.Parameters.Add("@M", SqlDbType.VarChar, 20).Value = dto.MemberCode;
                        cmdFH.Parameters.Add("@R", SqlDbType.VarChar, 30).Value = returnDocNo;
                        cmdFH.Parameters.Add("@T", SqlDbType.Decimal).Value = fineTotal;
                        cmdFH.Parameters.Add("@S", SqlDbType.VarChar, 1).Value = FineStatuses.Pending;
                        cmdFH.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = dto.ReturnedBy;
                        await cmdFH.ExecuteNonQueryAsync();
                    }

                    var fineLineNo = 1;
                    foreach (var fine in fineLines)
                    {
                        await using var cmdFD = new SqlCommand(@"
                        INSERT INTO dbo.T_TBLMEMBERFINE_D
                        (FD_DOCNO, FD_LINENO, FD_FINE_TYPE, FD_BOOKCODE, FD_QTY, FD_RATE, FD_AMOUNT, FD_REMARK, M_DATE)
                        VALUES
                        (@D, @L, @T, @B, @Q, @R, @A, @RM, SYSDATETIME());", con, tx);

                        cmdFD.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = fineDocNo;
                        cmdFD.Parameters.Add("@L", SqlDbType.Int).Value = fineLineNo++;
                        cmdFD.Parameters.Add("@T", SqlDbType.VarChar, 1).Value = Normalize(fine.FineType);
                        cmdFD.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = (object?)fine.BookCode ?? DBNull.Value;
                        cmdFD.Parameters.Add("@Q", SqlDbType.Int).Value = fine.Qty;
                        cmdFD.Parameters.Add("@R", SqlDbType.Decimal).Value = fine.Rate;
                        cmdFD.Parameters.Add("@A", SqlDbType.Decimal).Value = fine.Amount;
                        cmdFD.Parameters.Add("@RM", SqlDbType.NVarChar, 200).Value = (object?)fine.Remark ?? DBNull.Value;
                        await cmdFD.ExecuteNonQueryAsync();
                    }
                }

                await tx.CommitAsync();
                return new ReturnProcessResultDto(returnDocNo, fineDocNo, fineTotal);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private static string Normalize(string? value)
            => (value ?? string.Empty).Trim().ToUpperInvariant();
    }
    public static class ReturnStatuses
    {
        public const string Closed = "C";
        public const string Pending = "P";
        public const string Lost = "L";
    }
    // Add the following class to define the missing 'ReturnConditions' constants.
    public static class ReturnConditions
    {
        public const string Lost = "L";
        public const string Good = "G";
        public const string Damaged = "D";
        public const string Overdue = "O";
    }
}