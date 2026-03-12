using InvoiceManagement.DAL.Documents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoiceManagement.DAL.Repositories.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<InvoiceDocument?> GetByIdAsync(string id);
        Task<InvoiceDocument?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<IEnumerable<InvoiceDocument>> GetAllAsync(bool includeArchived = false);
        Task<IEnumerable<InvoiceDocument>> GetByCustomerIdAsync(string customerId);
        Task<IEnumerable<InvoiceDocument>> GetByStatusAsync(string status);
        Task<IEnumerable<InvoiceDocument>> GetOverdueAsync();
        Task<IEnumerable<InvoiceDocument>> GetDueBetweenAsync(DateTime from, DateTime to);
        Task<InvoiceDocument> CreateAsync(InvoiceDocument invoice);
        Task<InvoiceDocument> UpdateAsync(InvoiceDocument invoice);
        Task<bool> DeleteAsync(string id);
        Task<bool> ArchiveAsync(string id);
        Task<string> GenerateInvoiceNumberAsync();
        Task<long> CountByStatusAsync(string status);
    }

    public interface ICustomerRepository
    {
        Task<CustomerDocument?> GetByIdAsync(string id);
        Task<IEnumerable<CustomerDocument>> GetAllAsync(bool activeOnly = true);
        Task<IEnumerable<CustomerDocument>> SearchByNameAsync(string name);
        Task<CustomerDocument> CreateAsync(CustomerDocument customer);
        Task<CustomerDocument> UpdateAsync(CustomerDocument customer);
        Task<bool> SoftDeleteAsync(string id);
    }

    public interface IPaymentRepository
    {
        Task<PaymentDocument?> GetByIdAsync(string id);
        Task<IEnumerable<PaymentDocument>> GetByInvoiceIdAsync(string invoiceId);
        Task<IEnumerable<PaymentDocument>> GetByCustomerIdAsync(string customerId);
        Task<IEnumerable<PaymentDocument>> GetByDateRangeAsync(DateTime from, DateTime to);
        Task<PaymentDocument> CreateAsync(PaymentDocument payment);
        Task<bool> MarkReversedAsync(string id, string reason);
        Task<decimal> GetTotalPaidForInvoiceAsync(string invoiceId);
    }

    public interface IPaymentMethodRepository
    {
        Task<IEnumerable<PaymentMethodDocument>> GetAllActiveAsync();
        Task<PaymentMethodDocument?> GetByIdAsync(string id);
        Task<PaymentMethodDocument> CreateAsync(PaymentMethodDocument method);
    }

    public interface IAuditRepository
    {
        Task LogAsync(AuditLogDocument log);
        Task<IEnumerable<AuditLogDocument>> GetByInvoiceIdAsync(string invoiceId);
        Task<IEnumerable<AuditLogDocument>> GetByEventTypeAsync(string eventType, DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<AuditLogDocument>> GetRecentAsync(int count = 50);
    }

    public interface IReconciliationRepository
    {
        Task CreateAsync(ReconciliationDocument record);
        Task<IEnumerable<ReconciliationDocument>> GetByInvoiceIdAsync(string invoiceId);
        Task<IEnumerable<ReconciliationDocument>> GetByDateRangeAsync(DateTime from, DateTime to);
    }

    public interface IAnalyticsRepository
    {
        Task UpsertSnapshotAsync(AnalyticsSnapshotDocument snapshot);
        Task<AnalyticsSnapshotDocument?> GetByPeriodAsync(string period);
        Task<IEnumerable<AnalyticsSnapshotDocument>> GetByYearAsync(int year);
    }
}
