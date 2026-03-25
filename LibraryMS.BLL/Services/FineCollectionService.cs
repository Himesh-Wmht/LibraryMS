using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class FineCollectionService
    {
        private readonly FineCollectionRepository _repo;
        //public FineCollectionService(FineCollectionRepository repo) => _repo = repo;

        //public Task<List<MemberFineRowDto>> SearchOpenAsync(string? text)
        //    => _repo.SearchOpenAsync(text);

        //public Task PayAsync(FinePaymentDto dto)
        //    => _repo.PayAsync(dto);

        //public Task RefundAsync(FineRefundDto dto)
        //    => _repo.RefundAsync(dto);
        //public Task<List<FineDetailRowDto>> GetDetailsAsync(string fineDocNo)
        //    => _repo.GetDetailsAsync(fineDocNo);
        private readonly NotificationService _notifications;
        private readonly UserContactService _userContacts;

        public FineCollectionService(
            FineCollectionRepository repo,
            NotificationService notifications,
            UserContactService userContacts)
        {
            _repo = repo;
            _notifications = notifications;
            _userContacts = userContacts;
        }

        public Task<List<MemberFineRowDto>> SearchOpenAsync(string? text)
            => _repo.SearchOpenAsync(text);

        public Task<List<FineDetailRowDto>> GetDetailsAsync(string fineDocNo)
            => _repo.GetDetailsAsync(fineDocNo);

        public async Task PayAsync(FinePaymentDto dto)
        {
            await _repo.PayAsync(dto);

            var fine = await _repo.GetByDocNoAsync(dto.FineDocNo);
            if (fine == null) return;

            var details = await _repo.GetDetailsAsync(dto.FineDocNo);
            var contact = await _userContacts.GetByUserCodeAsync(fine.MemberCode);

            if (contact != null && !string.IsNullOrWhiteSpace(contact.Email))
            {
                var body = BuildPaymentEmailBody(
                    memberName: contact.UserName,
                    fine: fine,
                    paidAmount: dto.Amount,
                    payMode: dto.PayMode,
                    refNo: dto.RefNo,
                    details: details
                );

                await _notifications.QueueBothAsync(
                    eventType: "FINE_PAYMENT_RECEIVED",
                    refDocNo: fine.FineDocNo,
                    userCode: fine.MemberCode,
                    emailTo: contact.Email,
                    subject: $"Fine Payment Received - {fine.FineDocNo}",
                    body: body
                );

                await _notifications.ProcessPendingAsync();
            }
        }

        public async Task RefundAsync(FineRefundDto dto)
        {
            await _repo.RefundAsync(dto);

            var fine = await _repo.GetByDocNoAsync(dto.FineDocNo);
            if (fine == null) return;

            var details = await _repo.GetDetailsAsync(dto.FineDocNo);
            var contact = await _userContacts.GetByUserCodeAsync(fine.MemberCode);

            if (contact != null && !string.IsNullOrWhiteSpace(contact.Email))
            {
                var body = BuildRefundEmailBody(
                    memberName: contact.UserName,
                    fine: fine,
                    refundAmount: dto.Amount,
                    refundMode: dto.Mode,
                    reason: dto.Reason,
                    details: details
                );

                await _notifications.QueueBothAsync(
                    eventType: "FINE_REFUNDED",
                    refDocNo: fine.FineDocNo,
                    userCode: fine.MemberCode,
                    emailTo: contact.Email,
                    subject: $"Fine Refund Processed - {fine.FineDocNo}",
                    body: body
                );

                await _notifications.ProcessPendingAsync();
            }
        }

        private static string BuildPaymentEmailBody(
            string memberName,
            MemberFineRowDto fine,
            decimal paidAmount,
            string payMode,
            string? refNo,
            List<FineDetailRowDto> details)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Dear {memberName},");
            sb.AppendLine();
            sb.AppendLine("Your fine payment has been received successfully.");
            sb.AppendLine();
            sb.AppendLine($"Fine Document : {fine.FineDocNo}");
            sb.AppendLine($"Reference Doc : {fine.RefDocNo}");
            sb.AppendLine($"Fine Date     : {fine.FineDate:yyyy-MM-dd}");
            sb.AppendLine($"Paid Amount   : {paidAmount:N2}");
            sb.AppendLine($"Payment Mode  : {payMode}");
            sb.AppendLine($"Reference No  : {refNo ?? "-"}");
            sb.AppendLine($"Fine Total    : {fine.Total:N2}");
            sb.AppendLine($"Total Paid    : {fine.Paid:N2}");
            sb.AppendLine($"Balance       : {fine.Balance:N2}");
            sb.AppendLine();
            sb.AppendLine("Fine Details:");
            sb.AppendLine("--------------------------------------------------");

            foreach (var d in details)
            {
                var typeText = d.FineType switch
                {
                    "O" => "Overdue",
                    "D" => "Damaged",
                    "L" => "Lost",
                    "X" => "Other",
                    _ => d.FineType
                };

                sb.AppendLine($"Type     : {typeText}");
                sb.AppendLine($"Book Code: {d.BookCode ?? "-"}");
                sb.AppendLine($"Qty      : {d.Qty}");
                sb.AppendLine($"Rate     : {d.Rate:N2}");
                sb.AppendLine($"Amount   : {d.Amount:N2}");
                sb.AppendLine($"Remarks  : {d.Remark ?? "-"}");
                sb.AppendLine("--------------------------------------------------");
            }

            sb.AppendLine();
            sb.AppendLine("Thank you.");
            sb.AppendLine("Library Management System");

            return sb.ToString();
        }

        private static string BuildRefundEmailBody(
            string memberName,
            MemberFineRowDto fine,
            decimal refundAmount,
            string refundMode,
            string reason,
            List<FineDetailRowDto> details)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Dear {memberName},");
            sb.AppendLine();
            sb.AppendLine("A fine refund has been processed.");
            sb.AppendLine();
            sb.AppendLine($"Fine Document : {fine.FineDocNo}");
            sb.AppendLine($"Reference Doc : {fine.RefDocNo}");
            sb.AppendLine($"Refund Amount : {refundAmount:N2}");
            sb.AppendLine($"Refund Mode   : {refundMode}");
            sb.AppendLine($"Reason        : {reason}");
            sb.AppendLine();
            sb.AppendLine("Fine Details:");
            sb.AppendLine("--------------------------------------------------");

            foreach (var d in details)
            {
                var typeText = d.FineType switch
                {
                    "O" => "Overdue",
                    "D" => "Damaged",
                    "L" => "Lost",
                    "X" => "Other",
                    _ => d.FineType
                };

                sb.AppendLine($"Type     : {typeText}");
                sb.AppendLine($"Book Code: {d.BookCode ?? "-"}");
                sb.AppendLine($"Qty      : {d.Qty}");
                sb.AppendLine($"Rate     : {d.Rate:N2}");
                sb.AppendLine($"Amount   : {d.Amount:N2}");
                sb.AppendLine($"Remarks  : {d.Remark ?? "-"}");
                sb.AppendLine("--------------------------------------------------");
            }

            sb.AppendLine();
            sb.AppendLine("Library Management System");

            return sb.ToString();
        }
    }
}