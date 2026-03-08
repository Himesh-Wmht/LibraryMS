using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.BLL.Models
{
    public sealed class Transaction
    {
        public static class ReservationStatuses
        {
            public const string Pending = "P";
            public const string Active = "A";
            public const string Rejected = "R";
            public const string Cancelled = "C";
            public const string Fulfilled = "F";
            public const string Expired = "E";
        }

        public static class BorrowStatuses
        {
            public const string Open = "O";
            public const string PartialReturn = "P";
            public const string Closed = "C";
            public const string Cancelled = "X";
            public const string LostClosed = "L";
        }

        public static class ReturnStatuses
        {
            public const string Open = "O";
            public const string Closed = "C";
            public const string Cancelled = "X";
        }

        public static class ReturnConditions
        {
            public const string Normal = "N";
            public const string Overdue = "O";
            public const string Damaged = "D";
            public const string Lost = "L";
        }

        public static class FineStatuses
        {
            public const string Pending = "P";
            public const string PartiallyPaid = "T";
            public const string Paid = "D";
            public const string Waived = "W";
            public const string Refunded = "R";
            public const string Cancelled = "X";
        }
        public sealed class LibraryRulesOptions
        {
            public BorrowingOptions Borrowing { get; set; } = new();
            public FineOptions Fines { get; set; } = new();
        }

        public sealed class BorrowingOptions
        {
            public int DefaultBorrowDays { get; set; }
            public int MaxBooksPerMember { get; set; }
            public bool AllowBorrowWithOverdue { get; set; }
            public bool AllowBorrowWithOutstandingFine { get; set; }
            public decimal OutstandingFineLimit { get; set; }
        }

        public sealed class FineOptions
        {
            public LateFineOptions LateFine { get; set; } = new();
            public DamageFineOptions DamageFine { get; set; } = new();
            public LostFineOptions LostFine { get; set; } = new();
        }

        public sealed class LateFineOptions
        {
            public bool Enabled { get; set; }
            public string Formula { get; set; } = "PER_DAY_PER_BOOK";
            public int GraceDays { get; set; }
            public decimal DailyRate { get; set; }
            public decimal MaxFinePerBook { get; set; }
        }

        public sealed class DamageFineOptions
        {
            public bool Enabled { get; set; }
            public string Formula { get; set; } = "PERCENT_OF_REPLACEMENT_COST";
            public decimal Percent { get; set; }
            public decimal FlatAmount { get; set; }
        }

        public sealed class LostFineOptions
        {
            public bool Enabled { get; set; }
            public string Formula { get; set; } = "REPLACEMENT_COST_PLUS_PROCESSING_FEE";
            public decimal PercentOfReplacementCost { get; set; }
            public decimal ProcessingFee { get; set; }
        }
    }
  
}
