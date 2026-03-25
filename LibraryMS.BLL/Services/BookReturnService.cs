using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LibraryMS.BLL.Models;
using LibraryMS.DAL.Repositories;
using static LibraryMS.DAL.Repositories.Dtos;

namespace LibraryMS.BLL.Services
{
    public sealed class BookReturnService
    {
        private readonly BookReturnRepository _repo;
        private readonly FineCalculatorService _fineCalculator;
        private readonly NotificationService _notifications;
        private readonly UserContactService _userContacts;

        public BookReturnService(BookReturnRepository repo, FineCalculatorService fineCalculator, NotificationService notifications, UserContactService userContacts)
        {
            _repo = repo;
            _fineCalculator = fineCalculator;
            _notifications = notifications;
            _userContacts = userContacts;
        }

        public async Task<ReturnProcessResultDto> CreateAsync(ReturnCreateDto dto)
        {
            var fineLines = new List<FineLineDto>();

            foreach (var line in dto.Lines)
            {
                var ctx = await _repo.GetBorrowLineAsync(dto.BorrowDocNo, line.BorrowLineNo)
                    ?? throw new InvalidOperationException($"Borrow line not found: {dto.BorrowDocNo}/{line.BorrowLineNo}");

                if (line.Qty > ctx.OutstandingQty)
                    throw new InvalidOperationException($"Return qty cannot exceed outstanding qty for book {line.BookCode}");

                var condition = Normalize(line.Condition);

                if (condition == ReturnConditions.Overdue)
                {
                    var overdueDays = Math.Max(0, (DateTime.Today.Date - ctx.DueDate.Date).Days);
                    var graceDays = 1; // or get from appsettings
                    var chargeableDays = Math.Max(0, overdueDays - graceDays);
                    var dailyRate = 20m; // or get from appsettings

                    var amount = _fineCalculator.CalculateLateFine(ctx.DueDate, DateTime.Today, line.Qty);

                    if (amount > 0)
                    {
                        var calcText = $"Overdue: {overdueDays} day(s) - Grace {graceDays} = {chargeableDays} chargeable day(s). " +
                                       $"{chargeableDays} x {dailyRate:N2} x Qty {line.Qty} = {amount:N2}";

                        fineLines.Add(new FineLineDto("O", line.BookCode, line.Qty, dailyRate, amount, calcText));
                    }
                }
                else if (condition == ReturnConditions.Damaged)
                {
                    var percent = 30m; // get from appsettings
                    var amount = _fineCalculator.CalculateDamageFine(ctx.ReplacementCost, line.Qty);

                    if (amount > 0)
                    {
                        var calcText = $"Damaged: {percent:N2}% x Replacement Cost {ctx.ReplacementCost:N2} x Qty {line.Qty} = {amount:N2}";
                        fineLines.Add(new FineLineDto("D", line.BookCode, line.Qty, ctx.ReplacementCost, amount, calcText));
                    }
                }
                else if (condition == ReturnConditions.Lost)
                {
                    var processingFee = 100m; // get from appsettings
                    var amount = _fineCalculator.CalculateLostFine(ctx.ReplacementCost, line.Qty);

                    if (amount > 0)
                    {
                        var calcText = $"Lost: Replacement Cost {ctx.ReplacementCost:N2} x Qty {line.Qty} + Processing Fee {processingFee:N2} x Qty {line.Qty} = {amount:N2}";
                        fineLines.Add(new FineLineDto("L", line.BookCode, line.Qty, ctx.ReplacementCost, amount, calcText));
                    }
                }
            }

            var result = await _repo.CreateAsync(dto, fineLines);

            // ✅ SEND EMAIL ONLY IF FINE EXISTS
            if (!string.IsNullOrWhiteSpace(result.FineDocNo) && result.FineTotal > 0)
            {
                var contact = await _userContacts.GetByUserCodeAsync(dto.MemberCode);

                if (contact != null && !string.IsNullOrWhiteSpace(contact.Email))
                {
                    var body = BuildFineEmailBody(
                        memberName: contact.UserName,
                        borrowDocNo: dto.BorrowDocNo,
                        returnDocNo: result.ReturnDocNo,
                        fineDocNo: result.FineDocNo!,
                        fineTotal: result.FineTotal,
                        fineLines: fineLines
                    );

                    await _notifications.QueueBothAsync(
                        eventType: "FINE_CREATED",
                        refDocNo: result.FineDocNo,
                        userCode: dto.MemberCode,
                        emailTo: contact.Email,
                        subject: $"Library Fine Generated - {result.FineDocNo}",
                        body: body
                    );

                    await _notifications.ProcessPendingAsync();
                }
            }

            return result;
        }

        private static string BuildFineEmailBody(
            string memberName,
            string borrowDocNo,
            string returnDocNo,
            string fineDocNo,
            decimal fineTotal,
            List<FineLineDto> fineLines)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Dear {memberName},");
            sb.AppendLine();
            sb.AppendLine("A fine has been generated for your returned library books.");
            sb.AppendLine();
            sb.AppendLine($"Borrow Document : {borrowDocNo}");
            sb.AppendLine($"Return Document : {returnDocNo}");
            sb.AppendLine($"Fine Document   : {fineDocNo}");
            sb.AppendLine($"Fine Total      : {fineTotal:N2}");
            sb.AppendLine();
            sb.AppendLine("Fine Details:");
            sb.AppendLine("--------------------------------------------------");

            foreach (var line in fineLines)
            {
                var fineTypeText = line.FineType switch
                {
                    "O" => "Overdue",
                    "D" => "Damaged",
                    "L" => "Lost",
                    _ => line.FineType
                };

                sb.AppendLine($"Type     : {fineTypeText}");
                sb.AppendLine($"Book Code: {line.BookCode}");
                sb.AppendLine($"Qty      : {line.Qty}");
                sb.AppendLine($"Rate     : {line.Rate:N2}");
                sb.AppendLine($"Amount   : {line.Amount:N2}");
                sb.AppendLine($"Remarks  : {line.Remark}");
                sb.AppendLine("--------------------------------------------------");
            }

            sb.AppendLine();
            sb.AppendLine("Please contact the library/admin for payment details.");
            sb.AppendLine();
            sb.AppendLine("Library Management System");

            return sb.ToString();
        }

        private static string Normalize(string? value)
            => (value ?? string.Empty).Trim().ToUpperInvariant();
    }
    
}