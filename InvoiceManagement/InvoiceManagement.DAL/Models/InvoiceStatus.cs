namespace InvoiceManagement.DAL.Models
{
    /// <summary>
    /// Represents the lifecycle states of an invoice.
    /// Valid transitions:
    ///   Draft → Sent → Overdue
    ///                → Partially Paid → Paid
    ///                → Paid
    ///   Any state → Cancelled (except Paid)
    /// </summary>
    public enum InvoiceStatus
    {
        Draft,
        Sent,
        Overdue,
        PartiallyPaid,
        Paid,
        Cancelled
    }

    /// <summary>
    /// Payment terms controlling the due date calculation.
    /// </summary>
    public enum PaymentTerms
    {
        Immediate,  // Due on invoice date
        Net15,      // Due 15 days after invoice date
        Net30,      // Due 30 days after invoice date
        Net60,      // Due 60 days after invoice date
        Net90       // Due 90 days after invoice date
    }
}
