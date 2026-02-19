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
              DateTime ApDate
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
    }
}
