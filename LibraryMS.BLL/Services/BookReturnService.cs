using System;
using System.Collections.Generic;
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

        public BookReturnService(BookReturnRepository repo, FineCalculatorService fineCalculator)
        {
            _repo = repo;
            _fineCalculator = fineCalculator;
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

            return await _repo.CreateAsync(dto, fineLines);
        }

        private static string Normalize(string? value)
            => (value ?? string.Empty).Trim().ToUpperInvariant();
    }
    
}