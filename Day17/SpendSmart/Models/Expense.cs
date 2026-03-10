using System.ComponentModel.DataAnnotations;

namespace SpendSmart.Models
{
    public class Expense
    {
        public int Id { get; set; }

        public decimal MoneyValue { get; set; }

        [Required]
        public string? Description { get; set; }
    }
}
