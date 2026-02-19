using InvoiceManagement.DAL.Models;

namespace InvoiceManagement.BLL.Services.Interfaces
{
    /// <summary>
    /// Business logic service for invoice operations.
    /// This is what the Console/API layer calls — never access repositories directly.
    /// </summary>
    public interface IInvoiceService
    {
        Task<Invoice> CreateInvoiceAsync(Invoice invoice, List<InvoiceLineItem> lineItems);
        Task<Invoice?> GetInvoiceByIdAsync(int invoiceId);
        Task<Invoice?> GetInvoiceByNumberAsync(string invoiceNumber);
        Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
        Task<IEnumerable<Invoice>> GetByCustomerAsync(int customerId);
        Task<Invoice> UpdateInvoiceStatusAsync(int invoiceId, InvoiceStatus newStatus);
        Task<Invoice> AddLineItemAsync(int invoiceId, InvoiceLineItem lineItem);
        Task<bool> RemoveLineItemAsync(int invoiceId, int lineItemId);
        Task<bool> ArchiveInvoiceAsync(int invoiceId);
        Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
        Task<Dictionary<string, List<Invoice>>> GetAgingReportAsync();
        Task<decimal> GetDSOAsync(int periodDays);
        Task UpdateOverdueStatusesAsync();  // Called by a scheduler to auto-mark overdue
    }
}
