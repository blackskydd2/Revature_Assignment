using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace InvoiceManagement.DAL.Documents
{
    /// <summary>
    /// Customer master document
    /// </summary>
    public class CustomerDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [BsonElement("email")]
        public string? Email { get; set; }

        [BsonElement("phone")]
        public string? Phone { get; set; }

        [BsonElement("website")]
        public string? Website { get; set; }

        [BsonElement("industry")]
        public string? Industry { get; set; }

        [BsonElement("billingAddress")]
        public AddressDocument? BillingAddress { get; set; }

        [BsonElement("shippingAddress")]
        public AddressDocument? ShippingAddress { get; set; }

        [BsonElement("classification")]
        public string Classification { get; set; } = "Active"; // Active, Inactive, VIP, Prospect

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("createdDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [BsonElement("modifiedDate")]
        public DateTime? ModifiedDate { get; set; }
    }

    public class AddressDocument
    {
        [BsonElement("street")]
        public string? Street { get; set; }

        [BsonElement("city")]
        public string? City { get; set; }

        [BsonElement("state")]
        public string? State { get; set; }

        [BsonElement("postalCode")]
        public string? PostalCode { get; set; }

        [BsonElement("country")]
        public string? Country { get; set; }
    }

    /// <summary>
    /// Payment document — separate collection, references invoice by ID
    /// </summary>
    public class PaymentDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("invoiceId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string InvoiceId { get; set; } = string.Empty;

        [BsonElement("invoiceNumber")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [BsonElement("customerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CustomerId { get; set; } = string.Empty;

        [BsonElement("paymentAmount")]
        public decimal PaymentAmount { get; set; }

        [BsonElement("paymentDate")]
        public DateTime PaymentDate { get; set; }

        [BsonElement("paymentMethodId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PaymentMethodId { get; set; } = string.Empty;

        [BsonElement("paymentMethodName")]
        public string PaymentMethodName { get; set; } = string.Empty;

        [BsonElement("referenceNumber")]
        public string? ReferenceNumber { get; set; }

        [BsonElement("notes")]
        public string? Notes { get; set; }

        [BsonElement("receivedDate")]
        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

        [BsonElement("isReversed")]
        public bool IsReversed { get; set; }

        [BsonElement("reversalReason")]
        public string? ReversalReason { get; set; }

        [BsonElement("createdDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Payment method lookup document
    /// </summary>
    public class PaymentMethodDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("methodName")]
        public string MethodName { get; set; } = string.Empty;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Auto-increment sequence helper for invoice number generation
    /// </summary>
    public class InvoiceSequenceDocument
    {
        [BsonId]
        public string Id { get; set; } = "invoice_sequence";

        [BsonElement("lastSequenceNumber")]
        public long LastSequenceNumber { get; set; }

        [BsonElement("prefix")]
        public string Prefix { get; set; } = "INV";

        [BsonElement("year")]
        public int Year { get; set; }
    }

    /// <summary>
    /// Audit log document — append only, never updated
    /// </summary>
    public class AuditLogDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("invoiceId")]
        public string? InvoiceId { get; set; }

        [BsonElement("invoiceNumber")]
        public string? InvoiceNumber { get; set; }

        [BsonElement("customerId")]
        public string? CustomerId { get; set; }

        [BsonElement("eventType")]
        public string EventType { get; set; } = string.Empty;

        [BsonElement("eventDescription")]
        public string EventDescription { get; set; } = string.Empty;

        [BsonElement("previousValue")]
        public string? PreviousValue { get; set; }

        [BsonElement("newValue")]
        public string? NewValue { get; set; }

        [BsonElement("performedBy")]
        public string PerformedBy { get; set; } = "System";

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [BsonElement("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Payment reconciliation snapshot — stored per payment event
    /// </summary>
    public class ReconciliationDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("invoiceId")]
        public string InvoiceId { get; set; } = string.Empty;

        [BsonElement("invoiceNumber")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [BsonElement("paymentId")]
        public string PaymentId { get; set; } = string.Empty;

        [BsonElement("paymentAmount")]
        public decimal PaymentAmount { get; set; }

        [BsonElement("paymentDate")]
        public DateTime PaymentDate { get; set; }

        [BsonElement("paymentMethod")]
        public string PaymentMethod { get; set; } = string.Empty;

        [BsonElement("invoiceTotal")]
        public decimal InvoiceTotal { get; set; }

        [BsonElement("balanceBefore")]
        public decimal BalanceBefore { get; set; }

        [BsonElement("balanceAfter")]
        public decimal BalanceAfter { get; set; }

        [BsonElement("isFullyPaid")]
        public bool IsFullyPaid { get; set; }

        [BsonElement("reconciledAt")]
        public DateTime ReconciledAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Monthly analytics snapshot document
    /// </summary>
    public class AnalyticsSnapshotDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("period")]
        public string Period { get; set; } = string.Empty; // "2024-01"

        [BsonElement("snapshotDate")]
        public DateTime SnapshotDate { get; set; } = DateTime.UtcNow;

        [BsonElement("totalInvoices")]
        public int TotalInvoices { get; set; }

        [BsonElement("totalRevenue")]
        public decimal TotalRevenue { get; set; }

        [BsonElement("totalPaid")]
        public decimal TotalPaid { get; set; }

        [BsonElement("totalOutstanding")]
        public decimal TotalOutstanding { get; set; }

        [BsonElement("dso")]
        public double DaysSalesOutstanding { get; set; }

        [BsonElement("statusBreakdown")]
        public Dictionary<string, int> StatusBreakdown { get; set; } = new();

        [BsonElement("agingBuckets")]
        public AgingBucketsDocument AgingBuckets { get; set; } = new();

        [BsonElement("topCustomers")]
        public List<TopCustomerEntry> TopCustomers { get; set; } = new();
    }

    public class AgingBucketsDocument
    {
        [BsonElement("current")]
        public decimal Current { get; set; }

        [BsonElement("days1To30")]
        public decimal Days1To30 { get; set; }

        [BsonElement("days31To60")]
        public decimal Days31To60 { get; set; }

        [BsonElement("days61To90")]
        public decimal Days61To90 { get; set; }

        [BsonElement("over90Days")]
        public decimal Over90Days { get; set; }
    }

    public class TopCustomerEntry
    {
        [BsonElement("customerId")]
        public string CustomerId { get; set; } = string.Empty;

        [BsonElement("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [BsonElement("totalRevenue")]
        public decimal TotalRevenue { get; set; }

        [BsonElement("invoiceCount")]
        public int InvoiceCount { get; set; }
    }
}
