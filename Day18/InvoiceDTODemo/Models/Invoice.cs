using System.ComponentModel.DataAnnotations;

namespace InvoiceDTODemo.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        public DateTime InvoiceDate { get; set; }
        public decimal Amount { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}