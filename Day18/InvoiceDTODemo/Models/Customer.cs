using System.ComponentModel.DataAnnotations;

namespace InvoiceDTODemo.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public ICollection<Invoice> Invoices { get; set; }
    }
}