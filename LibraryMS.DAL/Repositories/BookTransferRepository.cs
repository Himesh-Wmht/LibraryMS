using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public sealed class BookTransferRepository
    {
        private readonly SqlDb _db;
        public BookTransferRepository(SqlDb db) => _db = db;

        // ✅ Book lookup (All active books)
        public async Task<List<LookupItemDto>> LookupBooksAsync(string? text)
        {
            const string sql = @"
SELECT TOP (200) B_CODE, B_TITLE
FROM dbo.M_TBLBOOKS
WHERE ISNULL(B_ACTIVE,0)=1
  AND (@T IS NULL OR B_CODE LIKE '%' + @T + '%'
               OR B_TITLE LIKE '%' + @T + '%'
               OR ISNULL(B_ISBN,'') LIKE '%' + @T + '%')
ORDER BY B_TITLE;";

            var list = new List<LookupItemDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@T", SqlDbType.NVarChar, 200).Value = (object?)NullIfEmpty(text) ?? DBNull.Value;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(new LookupItemDto(r.GetString(0), r.GetString(1)));

            return list;
        }

        // ✅ Book lookup limited to FROM-location inventory (recommended for transfers)
        // This is the method your UI was calling.
        //        public async Task<List<LookupItemDto>> LookupBooksInLocAsync(string fromLoc, string? text)
        //        {
        //            const string sql = @"
        //SELECT TOP (200)
        //    i.BI_BOOKCODE,
        //    b.B_TITLE,
        //    CAST(ISNULL(i.BI_QTY,0) AS varchar(20)) AS QtyText
        //FROM dbo.T_TBLBOOKINVENTORY i
        //JOIN dbo.M_TBLBOOKS b ON b.B_CODE = i.BI_BOOKCODE
        //WHERE i.BI_LOCCODE=@L
        //  AND ISNULL(i.BI_ACTIVE,0)=1
        //  AND ISNULL(b.B_ACTIVE,0)=1
        //  AND (@T IS NULL OR i.BI_BOOKCODE LIKE '%' + @T + '%'
        //               OR b.B_TITLE LIKE '%' + @T + '%')
        //ORDER BY b.B_TITLE;";

        //            var list = new List<LookupItemDto>();
        //            await using var con = _db.CreateConnection();
        //            await using var cmd = new SqlCommand(sql, con);

        //            cmd.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = fromLoc;
        //            cmd.Parameters.Add("@T", SqlDbType.NVarChar, 200).Value = (object?)NullIfEmpty(text) ?? DBNull.Value;

        //            await con.OpenAsync();
        //            await using var r = await cmd.ExecuteReaderAsync();
        //            while (await r.ReadAsync())
        //            {
        //                list.Add(new LookupItemDto(
        //                    Code: r.GetString(0),
        //                    Name: r.GetString(1),
        //                    Extra: "Qty: " + r.GetString(2)
        //                ));
        //            }

        //            return list;
        //        }
        public async Task<List<LookupItemDto>> LookupBooksInLocAsync(string fromLoc, string? text)
        {
            const string sql = @"
WITH R AS
(
    SELECT
        BR_BOOKCODE,
        BR_LOCCODE,
        SUM(BR_QTY) AS Reserved
    FROM dbo.T_TBLBOOKRESERVATIONS
    WHERE BR_STATUS = 'A'
      AND BR_EXPIRES_ON >= CONVERT(date, SYSDATETIME())
    GROUP BY BR_BOOKCODE, BR_LOCCODE
)
SELECT TOP (200)
    i.BI_BOOKCODE,
    b.B_TITLE,
    CAST(
        CASE
            WHEN ISNULL(i.BI_QTY, 0) - ISNULL(r.Reserved, 0) < 0 THEN 0
            ELSE ISNULL(i.BI_QTY, 0) - ISNULL(r.Reserved, 0)
        END
    AS int) AS AvailableQty,
    CAST(ISNULL(i.BI_QTY, 0) AS int) AS TotalQty,
    CAST(ISNULL(r.Reserved, 0) AS int) AS ReservedQty
FROM dbo.T_TBLBOOKINVENTORY i
JOIN dbo.M_TBLBOOKS b
    ON b.B_CODE = i.BI_BOOKCODE
LEFT JOIN R r
    ON r.BR_BOOKCODE = i.BI_BOOKCODE
   AND r.BR_LOCCODE = i.BI_LOCCODE
WHERE i.BI_LOCCODE = @L
  AND ISNULL(i.BI_ACTIVE, 0) = 1
  AND ISNULL(b.B_ACTIVE, 0) = 1
  AND (
        @T IS NULL
        OR i.BI_BOOKCODE LIKE '%' + @T + '%'
        OR b.B_TITLE LIKE '%' + @T + '%'
      )
  AND (
        CASE
            WHEN ISNULL(i.BI_QTY, 0) - ISNULL(r.Reserved, 0) < 0 THEN 0
            ELSE ISNULL(i.BI_QTY, 0) - ISNULL(r.Reserved, 0)
        END
      ) > 0
ORDER BY b.B_TITLE;";

            var list = new List<LookupItemDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = fromLoc;
            cmd.Parameters.Add("@T", SqlDbType.NVarChar, 200).Value = (object?)NullIfEmpty(text) ?? DBNull.Value;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();

            while (await r.ReadAsync())
            {
                var available = r.GetInt32(2);
                var total = r.GetInt32(3);
                var reserved = r.GetInt32(4);

                list.Add(new LookupItemDto(
                    Code: r.GetString(0),
                    Name: r.GetString(1),
                    Extra: $"Avail: {available} / Total: {total} / Res: {reserved}"
                ));
            }

            return list;
        }
        public async Task<int> GetAvailableQtyAsync(string locCode, string bookCode)
        {
            const string sql = @"
WITH R AS
(
    SELECT
        BR_BOOKCODE,
        BR_LOCCODE,
        SUM(BR_QTY) AS Reserved
    FROM dbo.T_TBLBOOKRESERVATIONS
    WHERE BR_STATUS = 'A'
      AND BR_EXPIRES_ON >= CONVERT(date, SYSDATETIME())
    GROUP BY BR_BOOKCODE, BR_LOCCODE
)
SELECT
    CAST(
        CASE
            WHEN ISNULL(i.BI_QTY, 0) - ISNULL(r.Reserved, 0) < 0 THEN 0
            ELSE ISNULL(i.BI_QTY, 0) - ISNULL(r.Reserved, 0)
        END
    AS int) AS AvailableQty
FROM dbo.T_TBLBOOKINVENTORY i
LEFT JOIN R r
    ON r.BR_BOOKCODE = i.BI_BOOKCODE
   AND r.BR_LOCCODE = i.BI_LOCCODE
WHERE i.BI_LOCCODE = @L
  AND i.BI_BOOKCODE = @B
  AND ISNULL(i.BI_ACTIVE, 0) = 1;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = locCode;
            cmd.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = bookCode;

            await con.OpenAsync();
            var obj = await cmd.ExecuteScalarAsync();

            if (obj == null || obj == DBNull.Value)
                return 0;

            return Convert.ToInt32(obj);
        }
        // ✅ Create Transfer (Pending)
        public async Task<string> CreateAsync(TransferCreateDto dto)
        {
            var docNo = "TR" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            await using var con = _db.CreateConnection();
            await con.OpenAsync();
            await using var tx = con.BeginTransaction();

            try
            {
                // Insert header
                await using (var cmdH = new SqlCommand(@"
INSERT INTO dbo.T_TBLBOOKTRANSFER_H
(TH_DOCNO, TH_FROM_LOC, TH_TO_LOC, TH_REQ_BY, TH_REQ_DATE, TH_STATUS, TH_REMARK, M_DATE)
VALUES
(@D, @F, @T, @U, SYSDATETIME(), 'P', @R, SYSDATETIME());", con, tx))
                {
                    cmdH.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = docNo;
                    cmdH.Parameters.Add("@F", SqlDbType.VarChar, 20).Value = dto.FromLoc;
                    cmdH.Parameters.Add("@T", SqlDbType.VarChar, 20).Value = dto.ToLoc;
                    cmdH.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = dto.ReqBy;
                    cmdH.Parameters.Add("@R", SqlDbType.NVarChar, 200).Value = (object?)dto.Remark ?? DBNull.Value;

                    await cmdH.ExecuteNonQueryAsync();
                }

                // Insert detail lines
                int line = 1;
                foreach (var x in dto.Lines)
                {
                    await using var cmdD = new SqlCommand(@"
INSERT INTO dbo.T_TBLBOOKTRANSFER_D
(TD_DOCNO, TD_LINENO, TD_BOOKCODE, TD_QTY, M_DATE)
VALUES
(@D, @L, @B, @Q, SYSDATETIME());", con, tx);

                    cmdD.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = docNo;
                    cmdD.Parameters.Add("@L", SqlDbType.Int).Value = line++;
                    cmdD.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = x.BookCode;
                    cmdD.Parameters.Add("@Q", SqlDbType.Int).Value = x.Qty;

                    await cmdD.ExecuteNonQueryAsync();
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

        // ✅ Pending approvals (incoming to this location)
        public async Task<List<TransferHeaderRowDto>> GetPendingAsync(string toLoc, bool loadAll)
        {
            var sql = loadAll
                ? @"
SELECT TH_DOCNO, TH_FROM_LOC, TH_TO_LOC, TH_REQ_BY, TH_REQ_DATE, TH_STATUS, TH_REMARK
FROM dbo.T_TBLBOOKTRANSFER_H
WHERE TH_TO_LOC=@T
ORDER BY TH_REQ_DATE DESC;"
                : @"
SELECT TH_DOCNO, TH_FROM_LOC, TH_TO_LOC, TH_REQ_BY, TH_REQ_DATE, TH_STATUS, TH_REMARK
FROM dbo.T_TBLBOOKTRANSFER_H
WHERE TH_TO_LOC=@T AND TH_STATUS='P'
ORDER BY TH_REQ_DATE DESC;";

            var list = new List<TransferHeaderRowDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@T", SqlDbType.VarChar, 20).Value = toLoc;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new TransferHeaderRowDto(
                    DocNo: r.GetString(0),
                    FromLoc: r.GetString(1),
                    ToLoc: r.GetString(2),
                    ReqBy: r.GetString(3),
                    ReqDate: r.GetDateTime(4),
                    Status: r.GetString(5),
                    Remark: r.IsDBNull(6) ? null : r.GetString(6)
                ));
            }

            return list;
        }

        public async Task<List<TransferDetailRowDto>> GetDetailsAsync(string docNo)
        {
            const string sql = @"
SELECT d.TD_LINENO, d.TD_BOOKCODE, b.B_TITLE, d.TD_QTY
FROM dbo.T_TBLBOOKTRANSFER_D d
JOIN dbo.M_TBLBOOKS b ON b.B_CODE = d.TD_BOOKCODE
WHERE d.TD_DOCNO=@D
ORDER BY d.TD_LINENO;";

            var list = new List<TransferDetailRowDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = docNo;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new TransferDetailRowDto(
                    LineNo: r.GetInt32(0),
                    BookCode: r.GetString(1),
                    Title: r.GetString(2),
                    Qty: r.GetInt32(3)
                ));
            }

            return list;
        }

        // ✅ APPROVE: add books to destination inventory
        public async Task ApproveAsync(string docNo, string approvedBy)
        {
            const string sql = @"
                        BEGIN TRY
                            BEGIN TRAN;

                            DECLARE @FROM varchar(20),
                                    @TO   varchar(20);

                            SELECT
                                @FROM = TH_FROM_LOC,
                                @TO   = TH_TO_LOC
                            FROM dbo.T_TBLBOOKTRANSFER_H WITH (UPDLOCK, HOLDLOCK)
                            WHERE TH_DOCNO = @D
                              AND TH_STATUS = 'P';

                            IF @FROM IS NULL OR @TO IS NULL
                            BEGIN
                                RAISERROR('Transfer not pending.', 16, 1);
                                ROLLBACK TRAN;
                                RETURN;
                            END

                            DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
                            SELECT TD_BOOKCODE, TD_QTY
                            FROM dbo.T_TBLBOOKTRANSFER_D
                            WHERE TD_DOCNO = @D
                            ORDER BY TD_LINENO;

                            DECLARE @B varchar(20),
                                    @Q int,
                                    @OnHand int,
                                    @Reserved int,
                                    @Available int,
                                    @Msg nvarchar(4000);

                            OPEN cur;
                            FETCH NEXT FROM cur INTO @B, @Q;

                            WHILE @@FETCH_STATUS = 0
                            BEGIN
                                -- Source on-hand
                                SELECT @OnHand = ISNULL(BI_QTY, 0)
                                FROM dbo.T_TBLBOOKINVENTORY WITH (UPDLOCK, HOLDLOCK)
                                WHERE BI_BOOKCODE = @B
                                  AND BI_LOCCODE = @FROM
                                  AND ISNULL(BI_ACTIVE, 0) = 1;

                                IF @OnHand IS NULL
                                BEGIN
                                    SET @Msg = N'Source inventory not found for book ' + ISNULL(@B, '');
                                    RAISERROR(@Msg, 16, 1);
                                    CLOSE cur;
                                    DEALLOCATE cur;
                                    ROLLBACK TRAN;
                                    RETURN;
                                END

                                -- Active reservations at source
                                SELECT @Reserved = ISNULL(SUM(BR_QTY), 0)
                                FROM dbo.T_TBLBOOKRESERVATIONS
                                WHERE BR_BOOKCODE = @B
                                  AND BR_LOCCODE = @FROM
                                  AND BR_STATUS = 'A'
                                  AND BR_EXPIRES_ON >= CONVERT(date, SYSDATETIME());

                                SET @Available = ISNULL(@OnHand, 0) - ISNULL(@Reserved, 0);

                                IF @Available < @Q
                                BEGIN
                                    SET @Msg = N'Insufficient available qty for book ' + ISNULL(@B, '')
                                             + N'. Available: ' + CAST(ISNULL(@Available, 0) AS nvarchar(20))
                                             + N', Requested: ' + CAST(ISNULL(@Q, 0) AS nvarchar(20));
                                    RAISERROR(@Msg, 16, 1);
                                    CLOSE cur;
                                    DEALLOCATE cur;
                                    ROLLBACK TRAN;
                                    RETURN;
                                END

                                -- 1) Deduct from source
                                UPDATE dbo.T_TBLBOOKINVENTORY
                                SET BI_QTY = ISNULL(BI_QTY, 0) - @Q,
                                    M_DATE = SYSDATETIME()
                                WHERE BI_BOOKCODE = @B
                                  AND BI_LOCCODE = @FROM
                                  AND ISNULL(BI_ACTIVE, 0) = 1;

                                -- 2) Add to destination
                                IF EXISTS
                                (
                                    SELECT 1
                                    FROM dbo.T_TBLBOOKINVENTORY
                                    WHERE BI_BOOKCODE = @B
                                      AND BI_LOCCODE = @TO
                                )
                                BEGIN
                                    UPDATE dbo.T_TBLBOOKINVENTORY
                                    SET BI_QTY = ISNULL(BI_QTY, 0) + @Q,
                                        BI_ACTIVE = 1,
                                        M_DATE = SYSDATETIME()
                                    WHERE BI_BOOKCODE = @B
                                      AND BI_LOCCODE = @TO;
                                END
                                ELSE
                                BEGIN
                                    INSERT INTO dbo.T_TBLBOOKINVENTORY
                                    (BI_BOOKCODE, BI_LOCCODE, BI_QTY, BI_REORDER, BI_ACTIVE, M_DATE)
                                    VALUES
                                    (@B, @TO, @Q, 0, 1, SYSDATETIME());
                                END

                                FETCH NEXT FROM cur INTO @B, @Q;
                            END

                            CLOSE cur;
                            DEALLOCATE cur;

                            UPDATE dbo.T_TBLBOOKTRANSFER_H
                            SET TH_STATUS   = 'A',
                                TH_APP_BY   = @U,
                                TH_APP_DATE = SYSDATETIME(),
                                M_DATE      = SYSDATETIME()
                            WHERE TH_DOCNO = @D
                              AND TH_STATUS = 'P';

                            COMMIT TRAN;
                        END TRY
                        BEGIN CATCH
                            IF CURSOR_STATUS('local', 'cur') >= -1
                            BEGIN
                                CLOSE cur;
                                DEALLOCATE cur;
                            END

                            IF @@TRANCOUNT > 0
                                ROLLBACK TRAN;

                            THROW;
                        END CATCH;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.CommandTimeout = 120;

            cmd.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = docNo;
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = approvedBy;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RejectAsync(string docNo, string rejectedBy, string? remark)
        {
            const string sql = @"
UPDATE dbo.T_TBLBOOKTRANSFER_H
SET TH_STATUS='R',
    TH_REJ_BY=@U,
    TH_REJ_DATE=SYSDATETIME(),
    TH_REMARK=COALESCE(@R, TH_REMARK),
    M_DATE=SYSDATETIME()
WHERE TH_DOCNO=@D AND TH_STATUS='P';";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@D", SqlDbType.VarChar, 30).Value = docNo;
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = rejectedBy;
            cmd.Parameters.Add("@R", SqlDbType.NVarChar, 200).Value = (object?)remark ?? DBNull.Value;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}