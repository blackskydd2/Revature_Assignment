using Microsoft.EntityFrameworkCore;
using InvoiceManagementDemo.Models;

namespace InvoiceManagementDemo.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Server=.\\SQLEXPRESS;Database=InvoiceDbDemo;Trusted_Connection=True;TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // -----------------------------
            // Invoice Configuration
            // -----------------------------
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(i => i.InvoiceId);

                entity.Property(i => i.InvoiceNumber)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasMany(i => i.LineItems)
                      .WithOne(li => li.Invoice)
                      .HasForeignKey(li => li.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(i => i.Payments)
                      .WithOne(p => p.Invoice)
                      .HasForeignKey(p => p.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // -----------------------------
            // PaymentMethod Configuration
            // -----------------------------
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.HasKey(pm => pm.MethodId);

                entity.Property(pm => pm.MethodName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasData(
                    new PaymentMethod { MethodId = 1, MethodName = "Cash" },
                    new PaymentMethod { MethodId = 2, MethodName = "UPI" },
                    new PaymentMethod { MethodId = 3, MethodName = "Credit Card" }
                );
            });

            // -----------------------------
            // Payment Configuration
            // -----------------------------
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.PaymentId);

                entity.HasOne(p => p.Method)
                      .WithMany(m => m.Payments)
                      .HasForeignKey(p => p.MethodId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // -----------------------------
            // InvoiceLineItem Configuration
            // -----------------------------
            modelBuilder.Entity<InvoiceLineItem>(entity =>
            {
                entity.HasKey(li => li.LineItemId);
            });
        }
    }
}
