using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Core;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.DAL.Repositories
{
    public sealed class BookReservationRepository
    {
        private readonly SqlDb _db;
        public BookReservationRepository(SqlDb db) => _db = db;

        // Available = Inventory - Approved active reservations
        public async Task<List<BookAvailRowDto>> SearchAvailableAsync(string locCode, string? text)
        {
            const string sql = @"
        WITH R AS (
            SELECT BR_BOOKCODE, BR_LOCCODE, SUM(BR_QTY) AS Reserved
            FROM dbo.T_TBLBOOKRESERVATIONS
            WHERE BR_STATUS='A'
              AND BR_EXPIRES_ON >= CONVERT(date, SYSDATETIME())
            GROUP BY BR_BOOKCODE, BR_LOCCODE
        )
        SELECT
            i.BI_BOOKCODE,
            b.B_TITLE,
            ISNULL(i.BI_QTY,0) AS Qty,
            ISNULL(r.Reserved,0) AS Reserved,
            (ISNULL(i.BI_QTY,0) - ISNULL(r.Reserved,0)) AS Available
        FROM dbo.T_TBLBOOKINVENTORY i
        JOIN dbo.M_TBLBOOKS b ON b.B_CODE = i.BI_BOOKCODE
        LEFT JOIN R r ON r.BR_BOOKCODE=i.BI_BOOKCODE AND r.BR_LOCCODE=i.BI_LOCCODE
        WHERE i.BI_LOCCODE=@L
          AND ISNULL(i.BI_ACTIVE,0)=1
          AND ISNULL(b.B_ACTIVE,0)=1
          AND (@T IS NULL OR i.BI_BOOKCODE LIKE '%' + @T + '%'
                       OR b.B_TITLE LIKE '%' + @T + '%')
        ORDER BY b.B_TITLE;";

            var list = new List<BookAvailRowDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = locCode;
            cmd.Parameters.Add("@T", SqlDbType.NVarChar, 200).Value = (object?)NullIfEmpty(text) ?? DBNull.Value;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new BookAvailRowDto(
                    BookCode: r.GetString(0),
                    Title: r.GetString(1),
                    Qty: r.GetInt32(2),
                    Reserved: r.GetInt32(3),
                    Available: r.GetInt32(4)
                ));
            }
            return list;
        }

        // User page: my reservations
        public async Task<List<ResMyRowDto>> GetMyAsync(string userCode, string locCode)
        {
            const string sql = @"
            SELECT r.BR_ID, r.BR_BOOKCODE, b.B_TITLE, r.BR_QTY, r.BR_HOLD_DAYS, r.BR_REQ_DATE, r.BR_STATUS, r.BR_EXPIRES_ON
            FROM dbo.T_TBLBOOKRESERVATIONS r
            JOIN dbo.M_TBLBOOKS b ON b.B_CODE=r.BR_BOOKCODE
            WHERE r.BR_USERCODE=@U AND r.BR_LOCCODE=@L
            ORDER BY r.BR_REQ_DATE DESC;";

            var list = new List<ResMyRowDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;
            cmd.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = locCode;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new ResMyRowDto(
                    ResId: r.GetInt32(0),
                    BookCode: r.GetString(1),
                    Title: r.GetString(2),
                    Qty: r.GetInt32(3),
                    HoldDays: r.GetInt32(4),
                    ReqDate: r.GetDateTime(5),
                    Status: r.GetString(6),
                    ExpiresOn: r.IsDBNull(7) ? null : r.GetDateTime(7)
                ));
            }
            return list;
        }
        public async Task<List<ResMyRowDto>> GetActiveByUserAsync(string userCode, string locCode)
        {
            const string sql = @"
            SELECT r.BR_ID, r.BR_BOOKCODE, b.B_TITLE, r.BR_QTY, r.BR_HOLD_DAYS, r.BR_REQ_DATE, r.BR_STATUS, r.BR_EXPIRES_ON
            FROM dbo.T_TBLBOOKRESERVATIONS r
            JOIN dbo.M_TBLBOOKS b ON b.B_CODE = r.BR_BOOKCODE
            WHERE r.BR_USERCODE = @U
              AND r.BR_LOCCODE = @L
              AND r.BR_STATUS = 'A'
              AND r.BR_EXPIRES_ON >= CONVERT(date, SYSDATETIME())
            ORDER BY r.BR_REQ_DATE DESC;";

            var list = new List<ResMyRowDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;
            cmd.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = locCode;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new ResMyRowDto(
                    ResId: r.GetInt32(0),
                    BookCode: r.GetString(1),
                    Title: r.GetString(2),
                    Qty: r.GetInt32(3),
                    HoldDays: r.GetInt32(4),
                    ReqDate: r.GetDateTime(5),
                    Status: r.GetString(6),
                    ExpiresOn: r.IsDBNull(7) ? null : r.GetDateTime(7)
                ));
            }
            return list;
        }
        // Create request = Pending approval
        public async Task CreateRequestAsync(ReservationRequestDto dto)
        {
            const string sql = @"
            INSERT INTO dbo.T_TBLBOOKRESERVATIONS
            (BR_USERCODE, BR_BOOKCODE, BR_LOCCODE, BR_QTY, BR_HOLD_DAYS, BR_REQ_DATE, BR_STATUS, BR_REMARK, M_DATE)
            VALUES
            (@U, @B, @L, @Q, @D, SYSDATETIME(), 'P', @R, SYSDATETIME());";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = dto.UserCode;
            cmd.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = dto.BookCode;
            cmd.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = dto.LocCode;
            cmd.Parameters.Add("@Q", SqlDbType.Int).Value = dto.Qty;
            cmd.Parameters.Add("@D", SqlDbType.Int).Value = dto.HoldDays;
            cmd.Parameters.Add("@R", SqlDbType.NVarChar, 200).Value = (object?)dto.Remark ?? DBNull.Value;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // Admin: pending list (location-based)
        public async Task<List<ResPendingRowDto>> GetPendingAsync(string locCode, bool loadAll)
        {
            var sql = loadAll
                ? @"
                    SELECT r.BR_ID, r.BR_USERCODE, ISNULL(u.U_NAME,''), r.BR_BOOKCODE, b.B_TITLE, r.BR_LOCCODE, r.BR_QTY, r.BR_HOLD_DAYS, r.BR_REQ_DATE, r.BR_STATUS
                    FROM dbo.T_TBLBOOKRESERVATIONS r
                    JOIN dbo.M_TBLBOOKS b ON b.B_CODE=r.BR_BOOKCODE
                    LEFT JOIN dbo.M_TBLUSERS u ON u.U_CODE=r.BR_USERCODE
                    WHERE r.BR_LOCCODE=@L
                    ORDER BY r.BR_REQ_DATE DESC;"
                                    : @"
                    SELECT r.BR_ID, r.BR_USERCODE, ISNULL(u.U_NAME,''), r.BR_BOOKCODE, b.B_TITLE, r.BR_LOCCODE, r.BR_QTY, r.BR_HOLD_DAYS, r.BR_REQ_DATE, r.BR_STATUS
                    FROM dbo.T_TBLBOOKRESERVATIONS r
                    JOIN dbo.M_TBLBOOKS b ON b.B_CODE=r.BR_BOOKCODE
                    LEFT JOIN dbo.M_TBLUSERS u ON u.U_CODE=r.BR_USERCODE
                    WHERE r.BR_LOCCODE=@L AND r.BR_STATUS='P'
                    ORDER BY r.BR_REQ_DATE DESC;";

            var list = new List<ResPendingRowDto>();
            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@L", SqlDbType.VarChar, 20).Value = locCode;

            await con.OpenAsync();
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new ResPendingRowDto(
                    ResId: r.GetInt32(0),
                    UserCode: r.GetString(1),
                    UserName: r.GetString(2),
                    BookCode: r.GetString(3),
                    Title: r.GetString(4),
                    LocCode: r.GetString(5),
                    Qty: r.GetInt32(6),
                    HoldDays: r.GetInt32(7),
                    ReqDate: r.GetDateTime(8),
                    Status: r.GetString(9)
                ));
            }
            return list;
        }

        // Approve: check availability again then mark Approved + set expiry
        public async Task ApproveAsync(int resId, string adminUserCode)
        {
            const string sql = @"
                BEGIN TRAN;

                DECLARE @B varchar(20), @L varchar(20), @Q int, @D int;

                SELECT @B=BR_BOOKCODE, @L=BR_LOCCODE, @Q=BR_QTY, @D=BR_HOLD_DAYS
                FROM dbo.T_TBLBOOKRESERVATIONS
                WHERE BR_ID=@I AND BR_STATUS='P';

                IF @B IS NULL
                BEGIN
                  ROLLBACK;
                  RAISERROR('Request not pending.',16,1);
                  RETURN;
                END

                DECLARE @Avail int;

                WITH R AS (
                    SELECT SUM(BR_QTY) Reserved
                    FROM dbo.T_TBLBOOKRESERVATIONS
                    WHERE BR_BOOKCODE=@B AND BR_LOCCODE=@L
                      AND BR_STATUS='A'
                      AND BR_EXPIRES_ON >= CONVERT(date, SYSDATETIME())
                )
                SELECT @Avail = (ISNULL(i.BI_QTY,0) - ISNULL((SELECT Reserved FROM R),0))
                FROM dbo.T_TBLBOOKINVENTORY i
                WHERE i.BI_BOOKCODE=@B AND i.BI_LOCCODE=@L AND ISNULL(i.BI_ACTIVE,0)=1;

                IF @Avail IS NULL OR @Avail < @Q
                BEGIN
                  ROLLBACK;
                  RAISERROR('Not enough available quantity.',16,1);
                  RETURN;
                END

                UPDATE dbo.T_TBLBOOKRESERVATIONS
                SET BR_STATUS='A',
                    BR_EXPIRES_ON = DATEADD(day, @D, CONVERT(date, SYSDATETIME())),
                    BR_APPROVED_BY=@B2,
                    BR_APPROVED_AT=SYSDATETIME(),
                    M_DATE=SYSDATETIME()
                WHERE BR_ID=@I AND BR_STATUS='P';

                COMMIT;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.CommandTimeout = 120;

            cmd.Parameters.Add("@I", SqlDbType.Int).Value = resId;
            cmd.Parameters.Add("@B2", SqlDbType.VarChar, 20).Value = adminUserCode;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task<int> CancelByUserAsync(int resId, string userCode)
        {
            const string sql = @"
                            UPDATE dbo.T_TBLBOOKRESERVATIONS
                            SET BR_STATUS='C', M_DATE=SYSDATETIME()
                            WHERE BR_ID=@I AND BR_USERCODE=@U AND BR_STATUS IN ('P','A');";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@I", SqlDbType.Int).Value = resId;
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }
        public async Task RejectAsync(int resId, string adminUserCode, string? remark)
        {
            const string sql = @"
                                UPDATE dbo.T_TBLBOOKRESERVATIONS
                                SET BR_STATUS='R',
                                    BR_REMARK=@R,
                                    BR_REJECTED_BY=@B,
                                    BR_REJECTED_AT=SYSDATETIME(),
                                    M_DATE=SYSDATETIME()
                                WHERE BR_ID=@I AND BR_STATUS='P';";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@I", SqlDbType.Int).Value = resId;
            cmd.Parameters.Add("@B", SqlDbType.VarChar, 20).Value = adminUserCode;
            cmd.Parameters.Add("@R", SqlDbType.NVarChar, 200).Value = (object?)remark ?? DBNull.Value;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task<int> CancelByAdminAsync(int resId, string adminUserCode, string? remark)
        {
            const string sql = @"
                        UPDATE dbo.T_TBLBOOKRESERVATIONS
                        SET BR_STATUS='C',
                            BR_REMARK = COALESCE(@R, BR_REMARK),
                            M_DATE=SYSDATETIME()
                        WHERE BR_ID=@I AND BR_STATUS IN ('P','A');";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@I", SqlDbType.Int).Value = resId;
            cmd.Parameters.Add("@R", SqlDbType.NVarChar, 200).Value = (object?)remark ?? DBNull.Value;

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }
        public async Task ProcessAsync(int resId, string adminUserCode)
        {
            const string sql = @"
                                BEGIN TRAN;

                                DECLARE @B varchar(20), @L varchar(20), @Q int;

                                SELECT @B=BR_BOOKCODE, @L=BR_LOCCODE, @Q=BR_QTY
                                FROM dbo.T_TBLBOOKRESERVATIONS
                                WHERE BR_ID=@I AND BR_STATUS='A';

                                IF @B IS NULL
                                BEGIN
                                  ROLLBACK;
                                  RAISERROR('Only APPROVED reservations can be processed.', 16, 1);
                                  RETURN;
                                END

                                UPDATE dbo.T_TBLBOOKINVENTORY
                                SET BI_QTY = BI_QTY - @Q,
                                    M_DATE = SYSDATETIME()
                                WHERE BI_BOOKCODE=@B AND BI_LOCCODE=@L
                                  AND ISNULL(BI_ACTIVE,0)=1
                                  AND ISNULL(BI_QTY,0) >= @Q;

                                IF @@ROWCOUNT = 0
                                BEGIN
                                  ROLLBACK;
                                  RAISERROR('Not enough stock in inventory to process this reservation.', 16, 1);
                                  RETURN;
                                END

                                UPDATE dbo.T_TBLBOOKRESERVATIONS
                                SET BR_STATUS='F',
                                    BR_PROC_BY=@U,
                                    BR_PROC_AT=SYSDATETIME(),
                                    M_DATE=SYSDATETIME()
                                WHERE BR_ID=@I AND BR_STATUS='A';

                                COMMIT;";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);
            cmd.CommandTimeout = 120;

            cmd.Parameters.Add("@I", SqlDbType.Int).Value = resId;
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = adminUserCode;

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // User cancel (pending or approved)
        public async Task<int> CancelAsync(int resId, string userCode)
        {
            const string sql = @"
                UPDATE dbo.T_TBLBOOKRESERVATIONS
                SET BR_STATUS='C', M_DATE=SYSDATETIME()
                WHERE BR_ID=@I AND BR_USERCODE=@U AND BR_STATUS IN ('P','A');";

            await using var con = _db.CreateConnection();
            await using var cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@I", SqlDbType.Int).Value = resId;
            cmd.Parameters.Add("@U", SqlDbType.VarChar, 20).Value = userCode;

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}