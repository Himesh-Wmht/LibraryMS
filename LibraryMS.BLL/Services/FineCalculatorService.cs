using System;
using LibraryMS.BLL.Models;
using static LibraryMS.BLL.Models.Transaction;


namespace LibraryMS.BLL.Services
{
    public sealed class FineCalculatorService
    {
        private readonly LibraryRulesOptions _rules;

        public FineCalculatorService(LibraryRulesOptions rules)
        {
            _rules = rules;
        }

        public decimal CalculateLateFine(DateTime dueDate, DateTime returnDate, int qty)
        {
            var cfg = _rules.Fines.LateFine;
            if (!cfg.Enabled) return 0m;

            var overdueDays = Math.Max(0, (returnDate.Date - dueDate.Date).Days);
            var chargeableDays = Math.Max(0, overdueDays - cfg.GraceDays);

            return cfg.Formula.Trim().ToUpperInvariant() switch
            {
                "PER_DAY_PER_BOOK" => Math.Min(chargeableDays * cfg.DailyRate * qty, cfg.MaxFinePerBook * qty),
                "PER_DAY_PER_TRANSACTION" => Math.Min(chargeableDays * cfg.DailyRate, cfg.MaxFinePerBook),
                "FLAT_AFTER_GRACE" => chargeableDays > 0 ? cfg.DailyRate : 0m,
                _ => 0m
            };
        }

        public decimal CalculateDamageFine(decimal replacementCost, int qty)
        {
            var cfg = _rules.Fines.DamageFine;
            if (!cfg.Enabled) return 0m;

            return cfg.Formula.Trim().ToUpperInvariant() switch
            {
                "PERCENT_OF_REPLACEMENT_COST" => replacementCost * (cfg.Percent / 100m) * qty,
                "FLAT_AMOUNT" => cfg.FlatAmount * qty,
                _ => 0m
            };
        }

        public decimal CalculateLostFine(decimal replacementCost, int qty)
        {
            var cfg = _rules.Fines.LostFine;
            if (!cfg.Enabled) return 0m;

            return cfg.Formula.Trim().ToUpperInvariant() switch
            {
                "REPLACEMENT_COST_ONLY" => replacementCost * qty,
                "REPLACEMENT_COST_PLUS_PROCESSING_FEE" => (replacementCost * qty) + (cfg.ProcessingFee * qty),
                "PERCENT_OF_REPLACEMENT_COST" => replacementCost * (cfg.PercentOfReplacementCost / 100m) * qty,
                _ => 0m
            };
        }
    }
}