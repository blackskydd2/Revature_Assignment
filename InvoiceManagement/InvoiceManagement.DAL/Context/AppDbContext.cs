using Microsoft.EntityFrameworkCore;
using InvoiceManagement.DAL.Models;

namespace InvoiceManagement.DAL.Context
{
    /// <summary>
    /// EF Core DbContext for the Invoice Management module.
    /// 
    /// DESIGN PHILOSOPHY:
    ///   - Data Annotations on models handle: Keys, Required, MaxLength, Column types,
    ///     FK declarations, Range validations.
    ///   - Fluent API here handles ONLY what annotations cannot:
    ///       1. Cascade delete behavior (Restrict on PaymentMethod)
    ///       2. Unique indexes (InvoiceNumber)
    ///       3. Seed data (PaymentMethod lookup table)
    ///       4. Enum-to-string conversions for readability in DB
    /// </summary>
    public class AppDbContext : DbContext
    {
        // ── DbSets ───────────────────────────────────────────────────────────
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

        // ── Constructor (supports DI and testing with InMemory provider) ─────
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ── Default constructor for console/migration use ─────────────────────
        public AppDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Only configure if not already configured (e.g., via DI in tests)
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=.\\SQLEXPRESS;Database=InvoiceManagementDB;Trusted_Connection=True;TrustServerCertificate=True"
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ================================================================
            // INVOICE CONFIGURATION
            // ================================================================

            modelBuilder.Entity<Invoice>(entity =>
            {
                // Unique constraint on InvoiceNumber — annotations cannot do this
                entity.HasIndex(i => i.InvoiceNumber)
                      .IsUnique()
                      .HasDatabaseName("UX_Invoices_InvoiceNumber");

                // Index on CustomerId for fast lookup of all invoices per customer
                entity.HasIndex(i => i.CustomerId)
                      .HasDatabaseName("IX_Invoices_CustomerId");

                // Index on Status for aging reports and dashboard queries
                entity.HasIndex(i => i.Status)
                      .HasDatabaseName("IX_Invoices_Status");

                // Index on DueDate for overdue detection queries
                entity.HasIndex(i => i.DueDate)
                      .HasDatabaseName("IX_Invoices_DueDate");

                // ── Relationship: Invoice → InvoiceLineItems ─────────────────
                // Cascade: deleting invoice deletes all line items
                entity.HasMany(i => i.LineItems)
                      .WithOne(li => li.Invoice)
                      .HasForeignKey(li => li.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);

                // ── Relationship: Invoice → Payments ─────────────────────────
                // Cascade: deleting invoice deletes all payment records
                entity.HasMany(i => i.Payments)
                      .WithOne(p => p.Invoice)
                      .HasForeignKey(p => p.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ================================================================
            // PAYMENT CONFIGURATION
            // ================================================================

            modelBuilder.Entity<Payment>(entity =>
            {
                // ── Relationship: Payment → PaymentMethod ────────────────────
                // RESTRICT: Cannot delete a PaymentMethod that has payments against it.
                // Annotations default to Cascade — we override here.
                entity.HasOne(p => p.Method)
                      .WithMany(m => m.Payments)
                      .HasForeignKey(p => p.MethodId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Index on InvoiceId for fast payment lookup per invoice
                entity.HasIndex(p => p.InvoiceId)
                      .HasDatabaseName("IX_Payments_InvoiceId");
            });

            // ================================================================
            // PAYMENT METHOD — SEED DATA
            // Annotations cannot seed data — must use Fluent API.
            // ================================================================

            modelBuilder.Entity<PaymentMethod>().HasData(
                new PaymentMethod { MethodId = 1, MethodName = "Cash",          Description = "Physical cash payment",                 IsActive = true },
                new PaymentMethod { MethodId = 2, MethodName = "UPI",           Description = "Unified Payments Interface (GPay, PhonePe, Paytm)", IsActive = true },
                new PaymentMethod { MethodId = 3, MethodName = "Credit Card",   Description = "Visa, Mastercard, Amex",                IsActive = true },
                new PaymentMethod { MethodId = 4, MethodName = "Debit Card",    Description = "Bank debit card",                       IsActive = true },
                new PaymentMethod { MethodId = 5, MethodName = "Bank Transfer", Description = "NEFT / RTGS / IMPS bank transfer",      IsActive = true },
                new PaymentMethod { MethodId = 6, MethodName = "Cheque",        Description = "Physical cheque payment",               IsActive = true }
            );
        }
    }
}
