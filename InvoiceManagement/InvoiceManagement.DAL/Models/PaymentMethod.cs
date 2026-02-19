using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceManagement.DAL.Models
{
    /// <summary>
    /// Lookup table for payment methods (Cash, UPI, Credit Card, Bank Transfer, etc.)
    /// 
    /// RELATIONSHIPS:
    ///   PaymentMethod (1) ──────────── (N) Payment
    ///   One payment method can be used across many payments.
    ///   DELETE RULE: Restrict — cannot delete a method if payments reference it.
    /// </summary>
    [Table("PaymentMethods")]
    public class PaymentMethod
    {
        // ── Primary Key ──────────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MethodId { get; set; }

        // ── Fields ───────────────────────────────────────────────────────────
        [Required(ErrorMessage = "Method name is required.")]
        [MaxLength(50, ErrorMessage = "Method name cannot exceed 50 characters.")]
        public string MethodName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // ── Navigation: One PaymentMethod → Many Payments ────────────────────
        // Cascade: Restrict (configured in AppDbContext — annotations cannot set Restrict)
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
