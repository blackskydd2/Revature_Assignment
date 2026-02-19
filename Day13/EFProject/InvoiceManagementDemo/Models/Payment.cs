namespace InvoiceManagementDemo.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public int MethodId { get; set; }

        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }

        public string ReferenceNumber { get; set; } = string.Empty;
        public DateTime ReceivedDate { get; set; }

        public Invoice Invoice { get; set; } = null!;
        public PaymentMethod Method { get; set; } = null!;
    }
}
