using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceManagement.DAL.Models
{
    /// <summary>
    /// Records a payment (full or partial) made against an invoice.
    /// 
    /// RELATIONSHIPS:
    ///   Payment (N) ──────────── (1) Invoice         [FK: InvoiceId]
    ///   Payment (N) ──────────── (1) PaymentMethod   [FK: MethodId]
    /// 
    ///   DELETE RULES:
    ///     Invoice   → Cascade  (payment removed when invoice is deleted)
    ///     PaymentMethod → Restrict (cannot delete method if payments exist)
    /// 
    /// PARTIAL PAYMENT SUPPORT:
    ///   Multiple payments can exist per invoice.
    ///   Invoice.AmountPaid = SUM of all Payment.PaymentAmount for that invoice.
    ///   Invoice status updated to PartiallyPaid or Paid by PaymentService.
    /// </summary>
    [Table("Payments")]
    public class Payment
    {
        // ── Primary Key ──────────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        // ── Foreign Key → Invoice ────────────────────────────────────────────
        [Required]
        [ForeignKey("Invoice")]
        public int InvoiceId { get; set; }

        // ── Foreign Key → PaymentMethod ─────────────────────────────────────
        [Required]
        [ForeignKey("Method")]
        public int MethodId { get; set; }

        // ── Payment Details ──────────────────────────────────────────────────
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than 0.")]
        public decimal PaymentAmount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Date the payment was physically received/cleared (may differ from PaymentDate).
        /// Useful for bank reconciliation.
        /// </summary>
        public DateTime ReceivedDate { get; set; }

        /// <summary>
        /// External reference: cheque number, transaction ID, UPI reference, etc.
        /// </summary>
        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }

        // ── Audit ────────────────────────────────────────────────────────────
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // ── Navigation: Many Payments → One Invoice ──────────────────────────
        public Invoice Invoice { get; set; } = null!;

        // ── Navigation: Many Payments → One PaymentMethod ───────────────────
        public PaymentMethod Method { get; set; } = null!;
    }
}
