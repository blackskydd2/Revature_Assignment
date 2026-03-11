using System.Collections.Generic;
using System.Threading.Tasks;
using InvoiceManagement.DAL.Models;

namespace InvoiceManagement.DAL.Repositories.Interfaces
{
    /// <summary>
    /// Invoice-specific repository operations beyond basic CRUD.
    /// </summary>
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<Invoice?> GetWithLineItemsAndPaymentsAsync(string id);   // signature only
        Task<IEnumerable<Invoice>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Invoice>> GetByStatusAsync(string status);
        Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
        Task<IEnumerable<Invoice>> GetByAgingBucketAsync(int daysOverdueMin, int daysOverdueMax);
        Task<decimal> GetTotalOutstandingByCustomerAsync(int customerId);
        Task<int> GetInvoiceCountForMonthAsync(int year, int month);
        Task<IEnumerable<Invoice>> GetArchivedInvoicesAsync();
        Task<bool> ArchiveInvoiceAsync(string id);
    }
}