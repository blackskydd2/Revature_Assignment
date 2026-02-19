using Microsoft.EntityFrameworkCore;
using InvoiceManagement.DAL.Context;
using InvoiceManagement.DAL.Models;
using InvoiceManagement.DAL.Repositories.Interfaces;

namespace InvoiceManagement.DAL.Repositories.Implementations
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _context;

        public InvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        // ── Basic CRUD ────────────────────────────────────────────────────────

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            // Excludes archived by default — use GetArchivedInvoicesAsync for archived
            return await _context.Invoices
                .Where(i => !i.IsArchived)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            return await _context.Invoices.FindAsync(id);
        }

        public async Task<Invoice?> GetWithLineItemsAndPaymentsAsync(int invoiceId)
        {
            // Eager load related entities — use this when you need full invoice detail
            return await _context.Invoices
                .Include(i => i.LineItems)
                .Include(i => i.Payments)
                    .ThenInclude(p => p.Method)   // Also load payment method name
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
        }

        public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            return await _context.Invoices
                .Include(i => i.LineItems)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
        }

        public async Task<Invoice> AddAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<Invoice> UpdateAsync(Invoice invoice)
        {
            invoice.ModifiedDate = DateTime.UtcNow;
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null) return false;

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Invoices.AnyAsync(i => i.InvoiceId == id);
        }

        // ── Lookup ────────────────────────────────────────────────────────────

        public async Task<IEnumerable<Invoice>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Invoices
                .Where(i => i.CustomerId == customerId && !i.IsArchived)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByStatusAsync(string status)
        {
            return await _context.Invoices
                .Where(i => i.Status == status && !i.IsArchived)
                .OrderByDescending(i => i.DueDate)
                .ToListAsync();
        }

        // ── Aging Report ─────────────────────────────────────────────────────

        public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Invoices
                .Where(i => i.DueDate < today
                         && i.Status != InvoiceStatus.Paid.ToString()
                         && i.Status != InvoiceStatus.Cancelled.ToString()
                         && !i.IsArchived)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetByAgingBucketAsync(int daysOverdueMin, int daysOverdueMax)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Invoices
                .Where(i => !i.IsArchived
                         && i.Status != InvoiceStatus.Paid.ToString()
                         && i.Status != InvoiceStatus.Cancelled.ToString()
                         && EF.Functions.DateDiffDay(i.DueDate, today) >= daysOverdueMin
                         && EF.Functions.DateDiffDay(i.DueDate, today) <= daysOverdueMax)
                .ToListAsync();
        }

        // ── Financial ────────────────────────────────────────────────────────

        public async Task<decimal> GetTotalOutstandingByCustomerAsync(int customerId)
        {
            return await _context.Invoices
                .Where(i => i.CustomerId == customerId
                         && i.Status != InvoiceStatus.Paid.ToString()
                         && i.Status != InvoiceStatus.Cancelled.ToString()
                         && !i.IsArchived)
                .SumAsync(i => i.OutstandingBalance);
        }

        // ── Invoice Number Generation ─────────────────────────────────────────

        public async Task<int> GetInvoiceCountForMonthAsync(int year, int month)
        {
            return await _context.Invoices
                .CountAsync(i => i.InvoiceDate.Year == year && i.InvoiceDate.Month == month);
        }

        // ── Archive ──────────────────────────────────────────────────────────

        public async Task<IEnumerable<Invoice>> GetArchivedInvoicesAsync()
        {
            return await _context.Invoices
                .Where(i => i.IsArchived)
                .OrderByDescending(i => i.ArchivedDate)
                .ToListAsync();
        }

        public async Task<bool> ArchiveInvoiceAsync(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) return false;

            invoice.IsArchived = true;
            invoice.ArchivedDate = DateTime.UtcNow;
            invoice.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
