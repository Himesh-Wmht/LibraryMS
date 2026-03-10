using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;

namespace LibraryMS.BLL.Services
{
    public sealed class ReportService
    {
        private readonly ReportRepository _repo;
        public ReportService(ReportRepository repo) => _repo = repo;

        public Task<DataTable> GetBorrowingIssuedAsync(string locCode, DateTime? from, DateTime? to) =>
            _repo.ExecuteReportAsync(
                "dbo.sp_RptBorrowingIssued",
                new SqlParameter("@LocCode", locCode),
                new SqlParameter("@DateFrom", (object?)from?.Date ?? DBNull.Value),
                new SqlParameter("@DateTo", (object?)to?.Date ?? DBNull.Value)
            );
        public Task<DataTable> GetOverdueItemsAsync(string locCode) =>
            _repo.ExecuteReportAsync(
                "dbo.sp_RptOverdueItems",
                new SqlParameter("@LocCode", locCode)
            );
        public Task<DataTable> GetMostBorrowedBooksAsync(string locCode, DateTime? from, DateTime? to, int topN) =>
            _repo.ExecuteReportAsync(
                "dbo.sp_RptMostBorrowedBooks",
                new SqlParameter("@LocCode", locCode),
                new SqlParameter("@DateFrom", (object?)from?.Date ?? DBNull.Value),
                new SqlParameter("@DateTo", (object?)to?.Date ?? DBNull.Value),
                new SqlParameter("@TopN", topN <= 0 ? 20 : topN)
            );
        public Task<DataTable> GetFineSummaryAsync(string locCode, DateTime? from, DateTime? to) =>
            _repo.ExecuteReportAsync(
                "dbo.sp_RptFineSummary",
                new SqlParameter("@LocCode", locCode),
                new SqlParameter("@DateFrom", (object?)from?.Date ?? DBNull.Value),
                new SqlParameter("@DateTo", (object?)to?.Date ?? DBNull.Value)
            );
        public Task<DataTable> GetMemberActivityAsync(string locCode, DateTime? from, DateTime? to) =>
                _repo.ExecuteReportAsync(
                    "dbo.sp_RptMemberActivity",
                    new SqlParameter("@LocCode", locCode),
                    new SqlParameter("@DateFrom", (object?)from?.Date ?? DBNull.Value),
                    new SqlParameter("@DateTo", (object?)to?.Date ?? DBNull.Value)
                );
        public Task<DataTable> GetBookAvailabilityAsync(string locCode) =>
            _repo.ExecuteReportAsync(
                "dbo.sp_RptBookAvailability",
                new SqlParameter("@LocCode", locCode)
            );
    }
}