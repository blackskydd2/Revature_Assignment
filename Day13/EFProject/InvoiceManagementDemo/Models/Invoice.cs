namespace InvoiceManagementDemo.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public int CustomerId { get; set; }
        public int QuoteId { get; set; }

        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }

        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal GrandTotal { get; set; }

        public DateTime CreatedDate { get; set; }

        public ICollection<InvoiceLineItem> LineItems { get; set; } = [];
        public ICollection<Payment> Payments { get; set; } = [];
    }
}
