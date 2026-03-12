using System;
using System.Collections.Generic;

namespace InvoiceManagement.BLL.Models
{
    // ── Request Models ────────────────────────────────────────────

    public class CreateInvoiceRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public string? QuoteId { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public string PaymentTerms { get; set; } = "Net 30";
        public string? Notes { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurringFrequency { get; set; }
        public decimal InvoiceDiscountAmount { get; set; } = 0;
        public decimal InvoiceTaxRatePercent { get; set; } = 0;
        public List<CreateLineItemRequest> LineItems { get; set; } = new();
        public string CreatedBy { get; set; } = "System";
    }

    public class CreateLineItemRequest
    {
        public string? ProductId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; } = 0;
        public decimal TaxRatePercent { get; set; } = 0;
    }

    public class ApplyPaymentRequest
    {
        public string InvoiceId { get; set; } = string.Empty;
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string PaymentMethodId { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public string ReceivedBy { get; set; } = "System";
    }

    public class CreateCustomerRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? Industry { get; set; }
        public string? BillingStreet { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingState { get; set; }
        public string? BillingPostalCode { get; set; }
        public string? BillingCountry { get; set; }
    }

    // ── Response / DTO Models ─────────────────────────────────────

    public class InvoiceSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal BalanceDue { get; set; }
        public int DaysOverdue { get; set; }
        public int LineItemCount { get; set; }
    }

    public class AgingReportDto
    {
        public DateTime ReportDate { get; set; } = DateTime.UtcNow;
        public decimal TotalOutstanding { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal Days1To30 { get; set; }
        public decimal Days31To60 { get; set; }
        public decimal Days61To90 { get; set; }
        public decimal Over90Days { get; set; }
        public List<AgingDetailDto> Details { get; set; } = new();
    }

    public class AgingDetailDto
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public decimal BalanceDue { get; set; }
        public int DaysOverdue { get; set; }
        public string AgingBucket { get; set; } = string.Empty;
    }

    public class DsoReportDto
    {
        public double DaysSalesOutstanding { get; set; }
        public decimal TotalReceivables { get; set; }
        public decimal AverageDailyRevenue { get; set; }
        public int PeriodDays { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    public class InvoiceCalculationResult
    {
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public List<LineItemCalcResult> LineItems { get; set; } = new();
    }

    public class LineItemCalcResult
    {
        public string? ProductId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxRatePercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class StatusTransitionResult
    {
        public bool IsValid { get; set; }
        public string? Reason { get; set; }
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
    }

    public class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; set; } = new();
        public void AddError(string msg) => Errors.Add(msg);
    }
}
