using InvoiceManagement.DAL.Models;

namespace InvoiceManagement.DAL.Repositories.Interfaces
{
    /// <summary>
    /// Payment-specific repository operations.
    /// </summary>
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByInvoiceIdAsync(int invoiceId);
        Task<decimal> GetTotalPaidForInvoiceAsync(int invoiceId);
        Task<IEnumerable<Payment>> GetPaymentsByMethodAsync(int methodId);
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to);
    }
}
