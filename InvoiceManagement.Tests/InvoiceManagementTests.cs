using InvoiceManagement.BLL.Engines;
using InvoiceManagement.BLL.Models;
using InvoiceManagement.BLL.Validators;
using InvoiceManagement.DAL.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace InvoiceManagement.Tests
{
    // ══════════════════════════════════════════════════════════════
    //  Calculation Engine Tests
    // ══════════════════════════════════════════════════════════════
    public class CalculationEngineTests
    {
        private readonly ICalculationEngine _engine = new CalculationEngine();

        [Fact]
        public void SingleItem_NoDiscountNoTax_ReturnsCorrectTotal()
        {
            var items = new[] { new CreateLineItemRequest { Description = "A", Quantity = 2, UnitPrice = 100 } };
            var result = _engine.Calculate(items);
            Assert.Equal(200, result.SubTotal);
            Assert.Equal(200, result.GrandTotal);
            Assert.Equal(0, result.TaxAmount);
        }

        [Fact]
        public void SingleItem_WithLineDiscount_ReducesLineTotal()
        {
            var items = new[] { new CreateLineItemRequest { Description = "B", Quantity = 1, UnitPrice = 100, DiscountPercent = 20 } };
            var result = _engine.Calculate(items);
            Assert.Equal(80, result.GrandTotal);
            Assert.Equal(20, result.LineItems[0].DiscountAmount);
        }

        [Fact]
        public void SingleItem_WithLineTax_AddsCorrectTax()
        {
            var items = new[] { new CreateLineItemRequest { Description = "C", Quantity = 1, UnitPrice = 100, TaxRatePercent = 10 } };
            var result = _engine.Calculate(items);
            Assert.Equal(10, result.TaxAmount);
            Assert.Equal(110, result.GrandTotal);
        }

        [Fact]
        public void InvoiceLevelDiscount_SubtractsFromGrandTotal()
        {
            var items = new[] { new CreateLineItemRequest { Description = "D", Quantity = 1, UnitPrice = 200 } };
            var result = _engine.Calculate(items, invoiceDiscountAmount: 50);
            Assert.Equal(150, result.GrandTotal);
            Assert.Equal(50, result.DiscountAmount);
        }

        [Fact]
        public void MultipleItems_SumsCorrectly()
        {
            var items = new[]
            {
                new CreateLineItemRequest { Description = "E", Quantity = 3, UnitPrice = 100 },
                new CreateLineItemRequest { Description = "F", Quantity = 2, UnitPrice = 50  }
            };
            var result = _engine.Calculate(items);
            Assert.Equal(400, result.GrandTotal);
        }

        [Fact]
        public void DiscountAndTax_Combined_CorrectResult()
        {
            // 2 × 100 = 200; 10% discount = -20 → 180; 10% tax = +18 → 198
            var total = _engine.CalculateLineTotal(2, 100, 10, 10);
            Assert.Equal(198, total);
        }

        [Fact]
        public void ZeroUnitPrice_ReturnsZeroTotal()
        {
            var items = new[] { new CreateLineItemRequest { Description = "Free", Quantity = 5, UnitPrice = 0 } };
            var result = _engine.Calculate(items);
            Assert.Equal(0, result.GrandTotal);
        }

        [Fact]
        public void LineItemsReturned_CountMatchesInput()
        {
            var items = Enumerable.Range(1, 5).Select(i =>
                new CreateLineItemRequest { Description = $"Item {i}", Quantity = 1, UnitPrice = 10 }).ToList();
            var result = _engine.Calculate(items);
            Assert.Equal(5, result.LineItems.Count);
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  Validator Tests
    // ══════════════════════════════════════════════════════════════
    public class ValidatorTests
    {
        private readonly IInvoiceValidator _v = new InvoiceValidator();

        [Fact]
        public void ValidCreateRequest_PassesValidation()
        {
            var req = new CreateInvoiceRequest
            {
                CustomerId = "abc123",
                InvoiceDate = DateTime.UtcNow.AddHours(-1),
                LineItems = new() { new CreateLineItemRequest { Description = "Item", Quantity = 1, UnitPrice = 100 } }
            };
            Assert.True(_v.ValidateCreate(req).IsValid);
        }

        [Fact]
        public void MissingCustomerId_FailsValidation()
        {
            var req = new CreateInvoiceRequest
            {
                CustomerId = "",
                InvoiceDate = DateTime.UtcNow,
                LineItems = new() { new CreateLineItemRequest { Description = "X", Quantity = 1, UnitPrice = 10 } }
            };
            var r = _v.ValidateCreate(req);
            Assert.False(r.IsValid);
            Assert.Contains(r.Errors, e => e.Contains("CustomerId"));
        }

        [Fact]
        public void NoLineItems_FailsValidation()
        {
            var req = new CreateInvoiceRequest { CustomerId = "abc", InvoiceDate = DateTime.UtcNow };
            Assert.False(_v.ValidateCreate(req).IsValid);
        }

        [Fact]
        public void NegativeQuantity_FailsValidation()
        {
            var req = new CreateInvoiceRequest
            {
                CustomerId = "abc",
                InvoiceDate = DateTime.UtcNow,
                LineItems = new() { new CreateLineItemRequest { Description = "X", Quantity = -1, UnitPrice = 10 } }
            };
            Assert.False(_v.ValidateCreate(req).IsValid);
        }

        [Fact]
        public void DiscountOver100Percent_FailsValidation()
        {
            var req = new CreateInvoiceRequest
            {
                CustomerId = "abc",
                InvoiceDate = DateTime.UtcNow,
                LineItems = new() { new CreateLineItemRequest { Description = "X", Quantity = 1, UnitPrice = 10, DiscountPercent = 110 } }
            };
            Assert.False(_v.ValidateCreate(req).IsValid);
        }

        [Fact]
        public void PaymentExceedsBalance_FailsValidation()
        {
            var req = new ApplyPaymentRequest { InvoiceId = "x", PaymentAmount = 500, PaymentDate = DateTime.UtcNow, PaymentMethodId = "y" };
            var r = _v.ValidatePayment(req, 100);
            Assert.False(r.IsValid);
            Assert.Contains(r.Errors, e => e.Contains("balance"));
        }

        [Fact]
        public void ZeroPaymentAmount_FailsValidation()
        {
            var req = new ApplyPaymentRequest { InvoiceId = "x", PaymentAmount = 0, PaymentDate = DateTime.UtcNow, PaymentMethodId = "y" };
            Assert.False(_v.ValidatePayment(req, 500).IsValid);
        }

        [Fact]
        public void ValidCustomer_PassesValidation()
        {
            var req = new CreateCustomerRequest { CustomerName = "Acme", Email = "a@b.com" };
            Assert.True(_v.ValidateCustomer(req).IsValid);
        }

        [Fact]
        public void EmptyCustomerName_FailsValidation()
        {
            var req = new CreateCustomerRequest { CustomerName = "" };
            Assert.False(_v.ValidateCustomer(req).IsValid);
        }

        [Fact]
        public void InvalidEmail_FailsValidation()
        {
            var req = new CreateCustomerRequest { CustomerName = "X", Email = "notanemail" };
            Assert.False(_v.ValidateCustomer(req).IsValid);
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  State Machine Tests
    // ══════════════════════════════════════════════════════════════
    public class StateMachineTests
    {
        private readonly IInvoiceStateMachine _sm = new InvoiceStateMachine();

        [Theory]
        [InlineData("Draft",        "Sent",          true)]
        [InlineData("Draft",        "Cancelled",     true)]
        [InlineData("Draft",        "Paid",          false)]
        [InlineData("Draft",        "Archived",      false)]
        [InlineData("Sent",         "Viewed",        true)]
        [InlineData("Sent",         "Paid",          true)]
        [InlineData("Sent",         "PartiallyPaid", true)]
        [InlineData("Sent",         "Overdue",       true)]
        [InlineData("Sent",         "Draft",         false)]
        [InlineData("PartiallyPaid","Paid",          true)]
        [InlineData("PartiallyPaid","Overdue",       true)]
        [InlineData("PartiallyPaid","Draft",         false)]
        [InlineData("Overdue",      "Paid",          true)]
        [InlineData("Overdue",      "Cancelled",     true)]
        [InlineData("Overdue",      "Draft",         false)]
        [InlineData("Paid",         "Archived",      true)]
        [InlineData("Paid",         "Draft",         false)]
        [InlineData("Archived",     "Draft",         false)]
        [InlineData("Archived",     "Paid",          false)]
        public void Transition_MatchesExpected(string from, string to, bool expected)
        {
            Assert.Equal(expected, _sm.CanTransition(from, to));
        }

        [Fact]
        public void InvalidTransition_IncludesReasonMessage()
        {
            var result = _sm.Validate("Draft", "Paid");
            Assert.False(result.IsValid);
            Assert.NotNull(result.Reason);
            Assert.Contains("Draft", result.Reason);
        }

        [Fact]
        public void ValidTransition_HasNullReason()
        {
            var result = _sm.Validate("Draft", "Sent");
            Assert.True(result.IsValid);
            Assert.Null(result.Reason);
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  Aging Engine Tests
    // ══════════════════════════════════════════════════════════════
    public class AgingEngineTests
    {
        private readonly IAgingEngine _engine = new AgingEngine();

        [Theory]
        [InlineData(-5,  "Current")]
        [InlineData(0,   "Current")]
        [InlineData(1,   "1-30")]
        [InlineData(30,  "1-30")]
        [InlineData(31,  "31-60")]
        [InlineData(60,  "31-60")]
        [InlineData(61,  "61-90")]
        [InlineData(90,  "61-90")]
        [InlineData(91,  "90+")]
        [InlineData(365, "90+")]
        public void GetBucket_ReturnsCorrectBucket(int days, string expectedBucket)
        {
            Assert.Equal(expectedBucket, _engine.GetBucket(days));
        }

        [Fact]
        public void PaidInvoices_ExcludedFromReport()
        {
            var invoices = new List<InvoiceDocument>
            {
                MakeInvoice(InvoiceStatus.Paid, DateTime.UtcNow.AddDays(-10), 500, 500)
            };
            var report = _engine.GenerateReport(invoices);
            Assert.Equal(0, report.TotalOutstanding);
            Assert.Empty(report.Details);
        }

        [Fact]
        public void OverdueInvoice_AppearsInCorrectBucket()
        {
            var invoices = new List<InvoiceDocument>
            {
                MakeInvoice(InvoiceStatus.Overdue, DateTime.UtcNow.AddDays(-45), 500, 0)
            };
            var report = _engine.GenerateReport(invoices);
            Assert.Equal(500, report.Days31To60);
            Assert.Equal(500, report.TotalOutstanding);
        }

        [Fact]
        public void PartiallyPaidInvoice_CorrectBalanceInReport()
        {
            var invoices = new List<InvoiceDocument>
            {
                MakeInvoice(InvoiceStatus.PartiallyPaid, DateTime.UtcNow.AddDays(5), 1000, 400)
            };
            var report = _engine.GenerateReport(invoices);
            Assert.Equal(600, report.CurrentAmount);
        }

        [Fact]
        public void MultipleInvoices_BucketsCorrectlyAggregated()
        {
            var invoices = new List<InvoiceDocument>
            {
                MakeInvoice(InvoiceStatus.Sent, DateTime.UtcNow.AddDays(-10), 200, 0), // 1-30
                MakeInvoice(InvoiceStatus.Sent, DateTime.UtcNow.AddDays(-50), 300, 0), // 31-60
                MakeInvoice(InvoiceStatus.Sent, DateTime.UtcNow.AddDays(10),  400, 0)  // Current
            };
            var report = _engine.GenerateReport(invoices);
            Assert.Equal(200, report.Days1To30);
            Assert.Equal(300, report.Days31To60);
            Assert.Equal(400, report.CurrentAmount);
            Assert.Equal(900, report.TotalOutstanding);
        }

        private static InvoiceDocument MakeInvoice(string status, DateTime dueDate, decimal total, decimal paid) =>
            new InvoiceDocument
            {
                InvoiceNumber = "INV-TEST",
                Status = status,
                DueDate = dueDate,
                GrandTotal = total,
                AmountPaid = paid,
                CustomerSnapshot = new CustomerSnapshot { CustomerName = "Test Corp" }
            };
    }

    // ══════════════════════════════════════════════════════════════
    //  DSO Engine Tests
    // ══════════════════════════════════════════════════════════════
    public class DsoEngineTests
    {
        private readonly IDsoEngine _engine = new DsoEngine();

        [Fact]
        public void NoInvoices_ReturnZeroDso()
        {
            var result = _engine.Calculate(Enumerable.Empty<InvoiceDocument>(), 30);
            Assert.Equal(0, result.DaysSalesOutstanding);
            Assert.Equal(0, result.TotalReceivables);
        }

        [Fact]
        public void AllPaidInvoices_ZeroReceivables()
        {
            var invoices = new List<InvoiceDocument>
            {
                new() { InvoiceDate = DateTime.UtcNow.AddDays(-10), GrandTotal = 1000, AmountPaid = 1000, Status = InvoiceStatus.Paid }
            };
            var result = _engine.Calculate(invoices, 30);
            Assert.Equal(0, result.TotalReceivables);
        }

        [Fact]
        public void UnpaidInvoice_HasPositiveDso()
        {
            var invoices = new List<InvoiceDocument>
            {
                new() { InvoiceDate = DateTime.UtcNow.AddDays(-10), GrandTotal = 3000, AmountPaid = 0, Status = InvoiceStatus.Sent }
            };
            var result = _engine.Calculate(invoices, 30);
            Assert.True(result.DaysSalesOutstanding > 0);
            Assert.Equal(3000, result.TotalReceivables);
        }
    }
}
