using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace InvoiceManagement.DAL.Documents
{
    /// <summary>
    /// Root invoice document stored in MongoDB.
    /// Contains embedded line items for atomic reads/writes.
    /// Payments are stored as a separate collection (referenced by InvoiceId).
    /// </summary>
    public class InvoiceDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("invoiceNumber")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [BsonElement("customerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CustomerId { get; set; } = string.Empty;

        [BsonElement("customerSnapshot")]
        public CustomerSnapshot CustomerSnapshot { get; set; } = new();

        [BsonElement("quoteId")]
        public string? QuoteId { get; set; }

        [BsonElement("invoiceDate")]
        public DateTime InvoiceDate { get; set; }

        [BsonElement("dueDate")]
        public DateTime DueDate { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = InvoiceStatus.Draft;

        [BsonElement("paymentTerms")]
        public string PaymentTerms { get; set; } = "Net 30";

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("subTotal")]
        public decimal SubTotal { get; set; }

        [BsonElement("discountAmount")]
        public decimal DiscountAmount { get; set; }

        [BsonElement("taxAmount")]
        public decimal TaxAmount { get; set; }

        [BsonElement("grandTotal")]
        public decimal GrandTotal { get; set; }

        [BsonElement("amountPaid")]
        public decimal AmountPaid { get; set; }

        [BsonElement("balanceDue")]
        public decimal BalanceDue => GrandTotal - AmountPaid;

        [BsonElement("lineItems")]
        public List<InvoiceLineItemDocument> LineItems { get; set; } = new();

        [BsonElement("isRecurring")]
        public bool IsRecurring { get; set; }

        [BsonElement("recurringFrequency")]
        public string? RecurringFrequency { get; set; }

        [BsonElement("sentDate")]
        public DateTime? SentDate { get; set; }

        [BsonElement("isArchived")]
        public bool IsArchived { get; set; }

        [BsonElement("createdDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [BsonElement("modifiedDate")]
        public DateTime? ModifiedDate { get; set; }

        [BsonElement("createdBy")]
        public string CreatedBy { get; set; } = "System";
    }

    /// <summary>
    /// Denormalized customer snapshot embedded in invoice
    /// so invoice history is preserved even if customer record changes
    /// </summary>
    public class CustomerSnapshot
    {
        [BsonElement("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [BsonElement("email")]
        public string? Email { get; set; }

        [BsonElement("phone")]
        public string? Phone { get; set; }

        [BsonElement("billingAddress")]
        public string? BillingAddress { get; set; }
    }

    /// <summary>
    /// Line item embedded within an invoice document
    /// </summary>
    public class InvoiceLineItemDocument
    {
        [BsonElement("lineItemId")]
        public string LineItemId { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("productId")]
        public string? ProductId { get; set; }

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("quantity")]
        public decimal Quantity { get; set; }

        [BsonElement("unitPrice")]
        public decimal UnitPrice { get; set; }

        [BsonElement("discountPercent")]
        public decimal DiscountPercent { get; set; }

        [BsonElement("discountAmount")]
        public decimal DiscountAmount { get; set; }

        [BsonElement("taxRatePercent")]
        public decimal TaxRatePercent { get; set; }

        [BsonElement("taxAmount")]
        public decimal TaxAmount { get; set; }

        [BsonElement("lineTotal")]
        public decimal LineTotal { get; set; }
    }

    /// <summary>
    /// Invoice status constants
    /// </summary>
    public static class InvoiceStatus
    {
        public const string Draft = "Draft";
        public const string Sent = "Sent";
        public const string Viewed = "Viewed";
        public const string PartiallyPaid = "PartiallyPaid";
        public const string Paid = "Paid";
        public const string Overdue = "Overdue";
        public const string Cancelled = "Cancelled";
        public const string Archived = "Archived";

        public static readonly IReadOnlyList<string> All = new[]
        {
            Draft, Sent, Viewed, PartiallyPaid, Paid, Overdue, Cancelled, Archived
        };
    }
}
