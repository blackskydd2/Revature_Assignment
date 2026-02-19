using InvoiceManagement.DAL.Models;

namespace InvoiceManagement.DAL.Repositories.Interfaces
{
    /// <summary>
    /// Invoice-specific repository operations beyond basic CRUD.
    /// </summary>
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        // ── Lookup ────────────────────────────────────────────────────────────
        Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<Invoice?> GetWithLineItemsAndPaymentsAsync(int invoiceId);
        Task<IEnumerable<Invoice>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Invoice>> GetByStatusAsync(string status);

        // ── Aging Report ─────────────────────────────────────────────────────
        Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
        Task<IEnumerable<Invoice>> GetByAgingBucketAsync(int daysOverdueMin, int daysOverdueMax);

        // ── Financial ────────────────────────────────────────────────────────
        Task<decimal> GetTotalOutstandingByCustomerAsync(int customerId);

        // ── Invoice Number Generation Support ────────────────────────────────
        Task<int> GetInvoiceCountForMonthAsync(int year, int month);

        // ── Archive ──────────────────────────────────────────────────────────
        Task<IEnumerable<Invoice>> GetArchivedInvoicesAsync();
        Task<bool> ArchiveInvoiceAsync(int invoiceId);
    }
}
