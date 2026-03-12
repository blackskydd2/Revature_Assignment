using InvoiceManagement.BLL.Engines;
using InvoiceManagement.BLL.Models;
using InvoiceManagement.BLL.Services.Interfaces;
using InvoiceManagement.BLL.Validators;
using InvoiceManagement.DAL.Documents;
using InvoiceManagement.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceManagement.BLL.Services.Implementations
{
    // ══════════════════════════════════════════════════════════════
    //  Customer Service
    // ══════════════════════════════════════════════════════════════
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repo;
        private readonly IInvoiceValidator _validator;
        private readonly IAuditRepository _auditRepo;

        public CustomerService(ICustomerRepository repo, IInvoiceValidator validator, IAuditRepository auditRepo)
        {
            _repo = repo;
            _validator = validator;
            _auditRepo = auditRepo;
        }

        public async Task<CustomerDocument> CreateAsync(CreateCustomerRequest request)
        {
            var v = _validator.ValidateCustomer(request);
            if (!v.IsValid) throw new ArgumentException(string.Join("; ", v.Errors));

            var customer = new CustomerDocument
            {
                CustomerName = request.CustomerName,
                Email = request.Email,
                Phone = request.Phone,
                Website = request.Website,
                Industry = request.Industry,
                BillingAddress = new AddressDocument
                {
                    Street = request.BillingStreet,
                    City = request.BillingCity,
                    State = request.BillingState,
                    PostalCode = request.BillingPostalCode,
                    Country = request.BillingCountry
                }
            };

            var created = await _repo.CreateAsync(customer);

            await _auditRepo.LogAsync(new AuditLogDocument
            {
                CustomerId = created.Id,
                EventType = "CustomerCreated",
                EventDescription = $"Customer '{created.CustomerName}' created.",
                NewValue = created.Id
            });

            return created;
        }

        public async Task<CustomerDocument?> GetByIdAsync(string id) => await _repo.GetByIdAsync(id);
        public async Task<IEnumerable<CustomerDocument>> GetAllAsync(bool activeOnly = true) => await _repo.GetAllAsync(activeOnly);
        public async Task<IEnumerable<CustomerDocument>> SearchAsync(string name) => await _repo.SearchByNameAsync(name);
        public async Task<CustomerDocument> UpdateAsync(CustomerDocument customer) => await _repo.UpdateAsync(customer);
        public async Task<bool> DeleteAsync(string id) => await _repo.SoftDeleteAsync(id);
    }

    // ══════════════════════════════════════════════════════════════
    //  Reporting Service
    // ══════════════════════════════════════════════════════════════
    public class ReportingService : IReportingService
    {
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly IAgingEngine _agingEngine;
        private readonly IDsoEngine _dsoEngine;
        private readonly IAuditRepository _auditRepo;
        private readonly IReconciliationRepository _reconRepo;
        private readonly IAnalyticsRepository _analyticsRepo;

        public ReportingService(
            IInvoiceRepository invoiceRepo,
            IAgingEngine agingEngine,
            IDsoEngine dsoEngine,
            IAuditRepository auditRepo,
            IReconciliationRepository reconRepo,
            IAnalyticsRepository analyticsRepo)
        {
            _invoiceRepo = invoiceRepo;
            _agingEngine = agingEngine;
            _dsoEngine = dsoEngine;
            _auditRepo = auditRepo;
            _reconRepo = reconRepo;
            _analyticsRepo = analyticsRepo;
        }

        public async Task<AgingReportDto> GetAgingReportAsync()
        {
            var invoices = await _invoiceRepo.GetAllAsync(false);
            return _agingEngine.GenerateReport(invoices);
        }

        public async Task<DsoReportDto> GetDsoAsync(int periodDays = 30)
        {
            var invoices = await _invoiceRepo.GetAllAsync(false);
            return _dsoEngine.Calculate(invoices, periodDays);
        }

        public async Task<AnalyticsSnapshotDocument> GenerateMonthlySnapshotAsync(int year, int month)
        {
            var all = (await _invoiceRepo.GetAllAsync(true)).ToList();
            var period = all.Where(i => i.InvoiceDate.Year == year && i.InvoiceDate.Month == month).ToList();

            var dso = _dsoEngine.Calculate(period, DateTime.DaysInMonth(year, month));
            var aging = _agingEngine.GenerateReport(period);

            var statusBreakdown = period
                .GroupBy(i => i.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var topCustomers = period
                .GroupBy(i => new { i.CustomerId, i.CustomerSnapshot.CustomerName })
                .Select(g => new TopCustomerEntry
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = g.Key.CustomerName,
                    TotalRevenue = g.Sum(i => i.GrandTotal),
                    InvoiceCount = g.Count()
                })
                .OrderByDescending(c => c.TotalRevenue)
                .Take(10)
                .ToList();

            var snapshot = new AnalyticsSnapshotDocument
            {
                Period = $"{year}-{month:D2}",
                SnapshotDate = DateTime.UtcNow,
                TotalInvoices = period.Count,
                TotalRevenue = period.Sum(i => i.GrandTotal),
                TotalPaid = period.Sum(i => i.AmountPaid),
                TotalOutstanding = period.Sum(i => i.GrandTotal - i.AmountPaid),
                DaysSalesOutstanding = dso.DaysSalesOutstanding,
                StatusBreakdown = statusBreakdown,
                TopCustomers = topCustomers,
                AgingBuckets = new AgingBucketsDocument
                {
                    Current = aging.CurrentAmount,
                    Days1To30 = aging.Days1To30,
                    Days31To60 = aging.Days31To60,
                    Days61To90 = aging.Days61To90,
                    Over90Days = aging.Over90Days
                }
            };

            await _analyticsRepo.UpsertSnapshotAsync(snapshot);
            return snapshot;
        }

        public async Task<IEnumerable<AnalyticsSnapshotDocument>> GetSnapshotsAsync(int year) =>
            await _analyticsRepo.GetByYearAsync(year);

        public async Task<IEnumerable<AuditLogDocument>> GetAuditTrailAsync(string invoiceId) =>
            await _auditRepo.GetByInvoiceIdAsync(invoiceId);

        public async Task<IEnumerable<ReconciliationDocument>> GetReconciliationsAsync(string invoiceId) =>
            await _reconRepo.GetByInvoiceIdAsync(invoiceId);
    }
}
