using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceManagement.DAL.Models
{
    /// <summary>
    /// Represents a single line item within an invoice (product or service).
    /// 
    /// RELATIONSHIPS:
    ///   InvoiceLineItem (N) ──────── (1) Invoice    [FK: InvoiceId]
    ///   Many line items belong to one invoice.
    ///   DELETE RULE: Cascade — deleting invoice removes all its line items.
    /// 
    /// FINANCIAL FORMULA:
    ///   LineTotal = (Quantity × UnitPrice) - Discount + Tax
    ///   Calculated by InvoiceCalculationEngine — do NOT set LineTotal manually.
    /// </summary>
    [Table("InvoiceLineItems")]
    public class InvoiceLineItem
    {
        // ── Primary Key ──────────────────────────────────────────────────────
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LineItemId { get; set; }

        // ── Foreign Key → Invoice ────────────────────────────────────────────
        // [ForeignKey] on the FK property, pointing to the navigation property name
        [Required]
        [ForeignKey("Invoice")]
        public int InvoiceId { get; set; }

        // ── Optional Product Reference (cross-module) ────────────────────────
        /// <summary>
        /// References Product in Stock/Inventory Management module. Nullable — service items
        /// may not have a product SKU.
        /// </summary>
        public int? ProductId { get; set; }

        // ── Item Details ─────────────────────────────────────────────────────
        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(300, ErrorMessage = "Description cannot exceed 300 characters.")]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SKU { get; set; }  // Stock-keeping unit if product-based

        // ── Quantity & Pricing ───────────────────────────────────────────────
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0.")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Line-level discount amount (not percentage).
        /// Applied per line before tax.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Discount cannot be negative.")]
        public decimal Discount { get; set; } = 0;

        /// <summary>
        /// Tax amount for this line. Calculated from TaxRate.
        /// Tax = (Quantity × UnitPrice - Discount) × TaxRate / 100
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; } = 0;

        /// <summary>
        /// Tax rate percentage (e.g., 18 for 18% GST). Stored for audit trail.
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal TaxRate { get; set; } = 0;

        /// <summary>
        /// LineTotal = (Quantity × UnitPrice) - Discount + Tax
        /// Set by InvoiceCalculationEngine only.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }

        // ── Navigation: Many InvoiceLineItems → One Invoice ──────────────────
        public Invoice Invoice { get; set; } = null!;
    }
}
