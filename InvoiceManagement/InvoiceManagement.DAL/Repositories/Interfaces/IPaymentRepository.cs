using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvoiceManagement.DAL.Models;

namespace InvoiceManagement.DAL.Repositories.Interfaces
{
    /// <summary>
    /// Payment-specific repository operations.
    /// </summary>
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsByInvoiceIdAsync(string invoiceId);
        Task<decimal> GetTotalPaidForInvoiceAsync(string invoiceId);
        Task<IEnumerable<Payment>> GetPaymentsByMethodAsync(string methodId);
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to);
    }
}