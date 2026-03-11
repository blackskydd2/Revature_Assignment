using InvoiceManagement.DAL.Models;

namespace InvoiceManagement.BLL.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<Invoice?> GetInvoiceByIdAsync(string id);
        Task<Invoice?> GetInvoiceByNumberAsync(string invoiceNumber);
        Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
        Task<IEnumerable<Invoice>> GetByCustomerAsync(int customerId);

        Task<Invoice> CreateInvoiceAsync(Invoice invoice, List<InvoiceLineItem> lineItems);
        Task<Invoice> UpdateInvoiceStatusAsync(string id, InvoiceStatus newStatus);
        Task<Invoice> AddLineItemAsync(string id, InvoiceLineItem lineItem);
        Task<bool> RemoveLineItemAsync(string id, string lineItemId);
        Task<bool> ArchiveInvoiceAsync(string id);

        Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
        Task<Dictionary<string, List<Invoice>>> GetAgingReportAsync();
        Task<decimal> GetDSOAsync(int periodDays = 30);
        Task UpdateOverdueStatusesAsync();
    }
}