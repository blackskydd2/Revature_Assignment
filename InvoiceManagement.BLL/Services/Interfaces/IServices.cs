using InvoiceManagement.BLL.Models;
using InvoiceManagement.DAL.Documents;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoiceManagement.BLL.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceDocument> CreateAsync(CreateInvoiceRequest request);
        Task<InvoiceDocument?> GetByIdAsync(string id);
        Task<InvoiceDocument?> GetByNumberAsync(string invoiceNumber);
        Task<IEnumerable<InvoiceSummaryDto>> GetAllAsync(bool includeArchived = false);
        Task<IEnumerable<InvoiceSummaryDto>> GetByCustomerAsync(string customerId);
        Task<IEnumerable<InvoiceSummaryDto>> GetOverdueAsync();
        Task<InvoiceDocument> ChangeStatusAsync(string id, string newStatus, string performedBy = "System");
        Task<InvoiceDocument> MarkSentAsync(string id, string recipientEmail, string performedBy = "System");
        Task<bool> ArchiveAsync(string id, string performedBy = "System");
        Task<bool> DeleteDraftAsync(string id);
        Task UpdateOverdueStatusesAsync();
    }

    public interface IPaymentService
    {
        Task<PaymentDocument> ApplyPaymentAsync(ApplyPaymentRequest request);
        Task<IEnumerable<PaymentDocument>> GetByInvoiceAsync(string invoiceId);
        Task<IEnumerable<PaymentDocument>> GetByDateRangeAsync(DateTime from, DateTime to);
        Task<bool> ReversePaymentAsync(string paymentId, string reason, string performedBy = "System");
        Task<IEnumerable<PaymentMethodDocument>> GetPaymentMethodsAsync();
    }

    public interface ICustomerService
    {
        Task<CustomerDocument> CreateAsync(CreateCustomerRequest request);
        Task<CustomerDocument?> GetByIdAsync(string id);
        Task<IEnumerable<CustomerDocument>> GetAllAsync(bool activeOnly = true);
        Task<IEnumerable<CustomerDocument>> SearchAsync(string name);
        Task<CustomerDocument> UpdateAsync(CustomerDocument customer);
        Task<bool> DeleteAsync(string id);
    }

    public interface IReportingService
    {
        Task<AgingReportDto> GetAgingReportAsync();
        Task<DsoReportDto> GetDsoAsync(int periodDays = 30);
        Task<AnalyticsSnapshotDocument> GenerateMonthlySnapshotAsync(int year, int month);
        Task<IEnumerable<AnalyticsSnapshotDocument>> GetSnapshotsAsync(int year);
        Task<IEnumerable<AuditLogDocument>> GetAuditTrailAsync(string invoiceId);
        Task<IEnumerable<ReconciliationDocument>> GetReconciliationsAsync(string invoiceId);
    }
}
