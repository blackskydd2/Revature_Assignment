using InvoiceManagement.DAL.Models;

namespace InvoiceManagement.BLL.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> RecordPaymentAsync(int invoiceId, decimal amount, int methodId, string? reference);
        Task<IEnumerable<Payment>> GetPaymentsByInvoiceAsync(int invoiceId);
        Task<decimal> GetOutstandingBalanceAsync(int invoiceId);
    }
}
