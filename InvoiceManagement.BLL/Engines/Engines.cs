using InvoiceManagement.BLL.Models;
using InvoiceManagement.DAL.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceManagement.BLL.Engines
{
    // ══════════════════════════════════════════════════════════════
    //  Calculation Engine
    //  Computes per-line totals, then rolls up to invoice totals
    // ══════════════════════════════════════════════════════════════
    public interface ICalculationEngine
    {
        InvoiceCalculationResult Calculate(
            IEnumerable<CreateLineItemRequest> items,
            decimal invoiceDiscountAmount = 0,
            decimal invoiceTaxRatePercent = 0);

        decimal CalculateLineTotal(decimal qty, decimal unitPrice, decimal discountPct, decimal taxPct);
    }

    public class CalculationEngine : ICalculationEngine
    {
        public InvoiceCalculationResult Calculate(
            IEnumerable<CreateLineItemRequest> items,
            decimal invoiceDiscountAmount = 0,
            decimal invoiceTaxRatePercent = 0)
        {
            var result = new InvoiceCalculationResult();
            var list = items.ToList();

            foreach (var item in list)
            {
                var gross = item.Quantity * item.UnitPrice;
                var lineDiscount = Math.Round(gross * (item.DiscountPercent / 100m), 2);
                var afterDiscount = gross - lineDiscount;
                var lineTax = Math.Round(afterDiscount * (item.TaxRatePercent / 100m), 2);
                var lineTotal = Math.Round(afterDiscount + lineTax, 2);

                result.LineItems.Add(new LineItemCalcResult
                {
                    ProductId = item.ProductId,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    DiscountPercent = item.DiscountPercent,
                    DiscountAmount = lineDiscount,
                    TaxRatePercent = item.TaxRatePercent,
                    TaxAmount = lineTax,
                    LineTotal = lineTotal
                });
            }

            result.SubTotal = result.LineItems.Sum(l => l.Quantity * l.UnitPrice);
            result.DiscountAmount = result.LineItems.Sum(l => l.DiscountAmount) + invoiceDiscountAmount;
            result.TaxAmount = result.LineItems.Sum(l => l.TaxAmount);

            // Invoice-level tax applied on net amount (after discounts)
            if (invoiceTaxRatePercent > 0)
            {
                var netBeforeInvTax = result.LineItems.Sum(l => l.LineTotal) - invoiceDiscountAmount;
                result.TaxAmount += Math.Round(netBeforeInvTax * (invoiceTaxRatePercent / 100m), 2);
            }

            result.GrandTotal = Math.Round(
                result.LineItems.Sum(l => l.LineTotal) - invoiceDiscountAmount +
                (invoiceTaxRatePercent > 0
                    ? Math.Round((result.LineItems.Sum(l => l.LineTotal) - invoiceDiscountAmount) * (invoiceTaxRatePercent / 100m), 2)
                    : 0),
                2);

            return result;
        }

        public decimal CalculateLineTotal(decimal qty, decimal unitPrice, decimal discountPct, decimal taxPct)
        {
            var gross = qty * unitPrice;
            var discount = Math.Round(gross * (discountPct / 100m), 2);
            var net = gross - discount;
            var tax = Math.Round(net * (taxPct / 100m), 2);
            return Math.Round(net + tax, 2);
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  Invoice Status State Machine
    // ══════════════════════════════════════════════════════════════
    public interface IInvoiceStateMachine
    {
        bool CanTransition(string from, string to);
        StatusTransitionResult Validate(string from, string to);
    }

    public class InvoiceStateMachine : IInvoiceStateMachine
    {
        private static readonly Dictionary<string, HashSet<string>> _transitions = new()
        {
            [InvoiceStatus.Draft]         = new() { InvoiceStatus.Sent, InvoiceStatus.Cancelled },
            [InvoiceStatus.Sent]          = new() { InvoiceStatus.Viewed, InvoiceStatus.Overdue, InvoiceStatus.Paid, InvoiceStatus.PartiallyPaid, InvoiceStatus.Cancelled },
            [InvoiceStatus.Viewed]        = new() { InvoiceStatus.Overdue, InvoiceStatus.Paid, InvoiceStatus.PartiallyPaid, InvoiceStatus.Cancelled },
            [InvoiceStatus.PartiallyPaid] = new() { InvoiceStatus.Paid, InvoiceStatus.Overdue },
            [InvoiceStatus.Overdue]       = new() { InvoiceStatus.Paid, InvoiceStatus.PartiallyPaid, InvoiceStatus.Cancelled },
            [InvoiceStatus.Paid]          = new() { InvoiceStatus.Archived },
            [InvoiceStatus.Cancelled]     = new() { InvoiceStatus.Archived },
            [InvoiceStatus.Archived]      = new()
        };

        public bool CanTransition(string from, string to) =>
            _transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

        public StatusTransitionResult Validate(string from, string to)
        {
            var ok = CanTransition(from, to);
            return new StatusTransitionResult
            {
                From = from,
                To = to,
                IsValid = ok,
                Reason = ok ? null : $"Cannot transition from '{from}' to '{to}'."
            };
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  Aging Engine
    // ══════════════════════════════════════════════════════════════
    public interface IAgingEngine
    {
        AgingReportDto GenerateReport(IEnumerable<InvoiceDocument> invoices);
        string GetBucket(int daysOverdue);
    }

    public class AgingEngine : IAgingEngine
    {
        public AgingReportDto GenerateReport(IEnumerable<InvoiceDocument> invoices)
        {
            var today = DateTime.UtcNow.Date;
            var report = new AgingReportDto { ReportDate = today };

            foreach (var inv in invoices)
            {
                if (inv.Status == InvoiceStatus.Paid || inv.Status == InvoiceStatus.Cancelled) continue;
                var balance = inv.GrandTotal - inv.AmountPaid;
                if (balance <= 0) continue;

                var days = (int)(today - inv.DueDate.Date).TotalDays;
                var bucket = GetBucket(days);

                report.Details.Add(new AgingDetailDto
                {
                    InvoiceNumber = inv.InvoiceNumber,
                    CustomerName = inv.CustomerSnapshot.CustomerName,
                    DueDate = inv.DueDate,
                    BalanceDue = balance,
                    DaysOverdue = Math.Max(0, days),
                    AgingBucket = bucket
                });

                report.TotalOutstanding += balance;
                switch (bucket)
                {
                    case "Current": report.CurrentAmount += balance; break;
                    case "1-30":    report.Days1To30     += balance; break;
                    case "31-60":   report.Days31To60    += balance; break;
                    case "61-90":   report.Days61To90    += balance; break;
                    case "90+":     report.Over90Days    += balance; break;
                }
            }

            return report;
        }

        public string GetBucket(int daysOverdue) => daysOverdue switch
        {
            <= 0  => "Current",
            <= 30 => "1-30",
            <= 60 => "31-60",
            <= 90 => "61-90",
            _     => "90+"
        };
    }

    // ══════════════════════════════════════════════════════════════
    //  DSO Engine
    //  DSO = (Accounts Receivable / Total Credit Sales) x Period Days
    // ══════════════════════════════════════════════════════════════
    public interface IDsoEngine
    {
        DsoReportDto Calculate(IEnumerable<InvoiceDocument> invoices, int periodDays = 30);
    }

    public class DsoEngine : IDsoEngine
    {
        public DsoReportDto Calculate(IEnumerable<InvoiceDocument> invoices, int periodDays = 30)
        {
            var today = DateTime.UtcNow.Date;
            var start = today.AddDays(-periodDays);

            var list = invoices.Where(i => i.InvoiceDate >= start && i.InvoiceDate <= today).ToList();
            var totalRevenue  = list.Sum(i => i.GrandTotal);
            var totalReceivables = list
                .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
                .Sum(i => i.GrandTotal - i.AmountPaid);

            var avgDaily = periodDays > 0 ? totalRevenue / periodDays : 0;
            var dso = avgDaily > 0 ? (double)(totalReceivables / avgDaily) : 0;

            return new DsoReportDto
            {
                DaysSalesOutstanding = Math.Round(dso, 2),
                TotalReceivables = totalReceivables,
                AverageDailyRevenue = Math.Round(avgDaily, 2),
                PeriodDays = periodDays,
                CalculatedAt = DateTime.UtcNow
            };
        }
    }
}
