using InvoiceManagement.DAL.Models;

namespace InvoiceManagement.BLL.Engines
{
    /// <summary>
    /// Handles all financial calculations for invoices.
    /// This is the single source of truth for invoice math — no calculation
    /// should happen in controllers, repositories, or models directly.
    /// 
    /// FORMULA CHAIN:
    ///   LineTotal      = (Quantity × UnitPrice) - LineDiscount + LineTax
    ///   SubTotal       = SUM(all LineTotals)
    ///   GrandTotal     = SubTotal - InvoiceDiscount + InvoiceTax
    ///   OutstandingBal = GrandTotal - AmountPaid
    /// </summary>
    public class InvoiceCalculationEngine
    {
        // ── Line Item Calculation ─────────────────────────────────────────────

        /// <summary>
        /// Calculates and sets Tax and LineTotal on a line item based on its TaxRate.
        /// Call this whenever Quantity, UnitPrice, Discount, or TaxRate changes.
        /// </summary>
        public void CalculateLineItem(InvoiceLineItem item)
        {
            decimal baseAmount = (item.Quantity * item.UnitPrice) - item.Discount;
            baseAmount = Math.Max(0, baseAmount); // Prevent negative base

            item.Tax = Math.Round(baseAmount * item.TaxRate / 100, 2);
            item.LineTotal = Math.Round(baseAmount + item.Tax, 2);
        }

        // ── Invoice-Level Calculation ─────────────────────────────────────────

        /// <summary>
        /// Recalculates SubTotal, Tax, GrandTotal, and OutstandingBalance
        /// for an invoice based on its line items.
        /// Must be called after any line item is added, updated, or removed.
        /// </summary>
        public void CalculateInvoiceTotals(Invoice invoice)
        {
            // Recalculate each line item first
            foreach (var item in invoice.LineItems)
                CalculateLineItem(item);

            // SubTotal = sum of all line totals BEFORE invoice-level discount
            decimal lineItemsTotal = invoice.LineItems.Sum(li => li.LineTotal);

            // Invoice-level discount applied after summing lines
            decimal afterDiscount = Math.Max(0, lineItemsTotal - invoice.Discount);

            // Tax already included in line totals — recalculate invoice-level tax sum
            invoice.Tax = invoice.LineItems.Sum(li => li.Tax);

            invoice.SubTotal = Math.Round(invoice.LineItems.Sum(li =>
                (li.Quantity * li.UnitPrice) - li.Discount), 2);

            invoice.GrandTotal = Math.Round(afterDiscount, 2);
            invoice.OutstandingBalance = Math.Round(invoice.GrandTotal - invoice.AmountPaid, 2);
        }

        // ── Due Date Calculation ──────────────────────────────────────────────

        /// <summary>
        /// Calculates DueDate based on InvoiceDate and selected PaymentTerms.
        /// </summary>
        public DateTime CalculateDueDate(DateTime invoiceDate, PaymentTerms terms)
        {
            return terms switch
            {
                PaymentTerms.Immediate => invoiceDate,
                PaymentTerms.Net15     => invoiceDate.AddDays(15),
                PaymentTerms.Net30     => invoiceDate.AddDays(30),
                PaymentTerms.Net60     => invoiceDate.AddDays(60),
                PaymentTerms.Net90     => invoiceDate.AddDays(90),
                _                      => invoiceDate.AddDays(30)
            };
        }

        // ── Aging Bucket Classification ───────────────────────────────────────

        /// <summary>
        /// Returns the aging bucket label for reporting.
        /// Buckets: Current | 1-30 days | 31-60 days | 61-90 days | 90+ days
        /// </summary>
        public string GetAgingBucket(Invoice invoice)
        {
            if (invoice.Status == InvoiceStatus.Paid.ToString() ||
                invoice.Status == InvoiceStatus.Cancelled.ToString())
                return "Closed";

            int daysOverdue = (DateTime.UtcNow.Date - invoice.DueDate.Date).Days;

            return daysOverdue switch
            {
                <= 0        => "Current",
                <= 30       => "1-30 Days Overdue",
                <= 60       => "31-60 Days Overdue",
                <= 90       => "61-90 Days Overdue",
                _           => "90+ Days Overdue"
            };
        }

        // ── DSO Calculation (Days Sales Outstanding) ──────────────────────────

        /// <summary>
        /// DSO = (Total Receivables / Total Revenue) × Number of Days in Period
        /// Measures average number of days to collect payment.
        /// </summary>
        public decimal CalculateDSO(decimal totalOutstanding, decimal totalRevenue, int periodDays)
        {
            if (totalRevenue == 0) return 0;
            return Math.Round((totalOutstanding / totalRevenue) * periodDays, 2);
        }
    }
}
