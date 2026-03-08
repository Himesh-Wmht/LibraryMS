using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMS.DAL.Repositories
{
    public class Dtos
    {
        public record UserGroupDto(string Code, string Name, decimal MembershipFee);
        public record SubscriptionDto(string Id, string Desc, int Days);
        public record LocationDto(string Code, string Desc);
        public sealed record ApprovalRowDto(
              string ApId,
              string UserCode,
              string? UserUid,
              string Name,
              string? Mobile,
              string GroupCode,
              string? SubType,
              decimal PaidAmt,
              decimal DueAmt,
              string? PaymentMethod,
              string? ReferenceNo,
              string? ApprovedBy,
              DateTime ApDate,
              bool IsApproved,
              bool IsRejected 
          );

        public sealed record ApprovalInsertDto(
            string UserCode,
            string ApId,
            string? UserUid,
            string Name,
            string? Mobile,
            string GroupCode,
            string? SubType,
            decimal PaidAmt,
            decimal DueAmt,
            string? PaymentMethod,
            string? ReferenceNo,
            string? ApprovedBy,
            DateTime ApDate,
            bool Processed,
            bool Canceled   
        );
        // ✅ Grid Row DTO (what UCGroupMenus binds to)
        public sealed record GroupMenuRowDto(
            string MenuCode,
            string MenuDesc,
            string? ParentCode,
            int ChildOrder,      // maps from M_CHILD
            bool Assigned,       // based on GP_STATUS
            string? RuleLoc      // 'ALL' / locCode / null
        );

        // ✅ Update DTO (what Save sends)
        public sealed record GroupMenuUpdateDto(
            string MenuCode,
            bool Assigned,
            string? Locs = null  // optional override; if null -> use current locCode
        );
        // ----- Book Catalog -----
        public sealed record BookCategoryDto(string Code, string Name, bool Active);
        // ✅ NEW: For category grid page
        public sealed record BookCategoryRowDto(string Code, string Name, bool Active);

        // ✅ NEW: For category save/upsert
        public sealed record BookCategoryUpsertDto(
            string Code,
            string Name,
            bool Active
        );
        public sealed record BookRowDto(
            string Code,
            string Title,
            string? Author,
            string? Publisher,
            string? Isbn,
            string? CategoryCode,
            string? CategoryName,
            decimal Price,
            bool Active
        );

        public sealed record BookUpsertDto(
            string Code,
            string Title,
            string? Author,
            string? Publisher,
            string? Isbn,
            string? CategoryCode,
            decimal Price,
            bool Active
        );

        // ----- Inventory -----
        public sealed record InvRowDto(
            int Id,
            string BookCode,
            string Title,
            string LocCode,
            int Qty,
            int Reorder,
            bool Active
        );

        public sealed record InvUpsertDto(
            string BookCode,
            string LocCode,
            int Qty,
            int Reorder,
            bool Active
        );
        public sealed record PwdResetRowDto(
            string PrId,
            string UserCode,
            string Name,
            string? Mobile,
            DateTime ReqDate,
            string Status,
            string? RequestedBy
        );
        public sealed record UserLockRowDto(
            string UlId,
            string UserCode,
            DateTime LockedAt,
            string Status
        );
        public sealed record LookupItemDto(string Code, string Name, string? Extra = null);
        public sealed record BookAvailRowDto(
           string BookCode,
           string Title,
           int Qty,
           int Reserved,
           int Available
        );

        public sealed record ResMyRowDto(
            int ResId,
            string BookCode,
            string Title,
            int Qty,
            int HoldDays,
            DateTime ReqDate,
            string Status,
            DateTime? ExpiresOn
        );

        public sealed record ResPendingRowDto(
            int ResId,
            string UserCode,
            string UserName,
            string BookCode,
            string Title,
            string LocCode,
            int Qty,
            int HoldDays,
            DateTime ReqDate,
            string Status
        );

        public sealed record ReservationRequestDto(
            string UserCode,
            string LocCode,
            string BookCode,
            int Qty,
            int HoldDays,
            string? Remark
        );
        public sealed record TransferLineDto(string BookCode, string Title, int Qty);
        public sealed record TransferHeaderRowDto(
            string DocNo,
            string FromLoc,
            string ToLoc,
            string ReqBy,
            DateTime ReqDate,
            string Status,
            string? Remark
        );
        public sealed record TransferDetailRowDto(
            int LineNo,
            string BookCode,
            string Title,
            int Qty
        );

        public sealed record TransferCreateDto(
            string FromLoc,
            string ToLoc,
            string ReqBy,
            string? Remark,
            List<TransferLineDto> Lines
        );
        public sealed record BorrowLineDto(
    string BookCode,
    string Title,
    int Qty,
    DateTime DueDate,
    int? ReservationId
);

        public sealed record BorrowCreateDto(
            string MemberCode,
            string LocCode,
            string BorrowedBy,
            string? Remark,
            List<BorrowLineDto> Lines
        );

        public sealed record BorrowOpenRowDto(
            string DocNo,
            string MemberCode,
            string MemberName,
            string LocCode,
            DateTime BorrowDate,
            string Status
        );

        public sealed record BorrowOpenDetailRowDto(
            int LineNo,
            string BookCode,
            string Title,
            int Qty,
            int ReturnedQty,
            int OutstandingQty,
            DateTime DueDate,
            decimal ReplacementCost,
            int? ReservationId
        );

        public sealed record ReturnLineDto(
            int BorrowLineNo,
            string BookCode,
            int Qty,
            string Condition,
            string? Remark
        );

        public sealed record ReturnCreateDto(
            string BorrowDocNo,
            string MemberCode,
            string LocCode,
            string ReturnedBy,
            string? Remark,
            List<ReturnLineDto> Lines
        );

        public sealed record FineLineDto(
            string FineType,
            string? BookCode,
            int Qty,
            decimal Rate,
            decimal Amount,
            string? Remark
        );

        public sealed record ReturnProcessResultDto(
            string ReturnDocNo,
            string? FineDocNo,
            decimal FineTotal
        );

        public sealed record MemberFineRowDto(
            string FineDocNo,
            string MemberCode,
            string RefType,
            string RefDocNo,
            DateTime FineDate,
            decimal Total,
            decimal Paid,
            decimal Balance,
            string Status,
            string? Remark
        );

        public sealed record FinePaymentDto(
            string FineDocNo,
            DateTime PayDate,
            string PayMode,
            decimal Amount,
            string? RefNo,
            string ReceivedBy,
            string? Remark
        );

        public sealed record FineRefundDto(
            string FineDocNo,
            DateTime RefundDate,
            decimal Amount,
            string Mode,
            string Reason,
            string ApprovedBy,
            string? Remark
        );
        public sealed record FineDetailRowDto(
            string FineType,
            string? BookCode,
            int Qty,
            decimal Rate,
            decimal Amount,
            string? Remark
        );
    }
}
