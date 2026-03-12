using InvoiceManagement.DAL.Context;
using InvoiceManagement.DAL.Documents;
using InvoiceManagement.DAL.Repositories.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoiceManagement.DAL.Repositories.Implementations
{
    // ══════════════════════════════════════════════════════════════
    //  Invoice Repository
    // ══════════════════════════════════════════════════════════════
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly MongoDbContext _ctx;
        public InvoiceRepository(MongoDbContext ctx) => _ctx = ctx;

        public async Task<InvoiceDocument?> GetByIdAsync(string id) =>
            await _ctx.Invoices.Find(i => i.Id == id).FirstOrDefaultAsync();

        public async Task<InvoiceDocument?> GetByInvoiceNumberAsync(string invoiceNumber) =>
            await _ctx.Invoices.Find(i => i.InvoiceNumber == invoiceNumber).FirstOrDefaultAsync();

        public async Task<IEnumerable<InvoiceDocument>> GetAllAsync(bool includeArchived = false)
        {
            var filter = includeArchived
                ? Builders<InvoiceDocument>.Filter.Empty
                : Builders<InvoiceDocument>.Filter.Eq(i => i.IsArchived, false);

            return await _ctx.Invoices.Find(filter)
                .SortByDescending(i => i.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<InvoiceDocument>> GetByCustomerIdAsync(string customerId) =>
            await _ctx.Invoices.Find(i => i.CustomerId == customerId && !i.IsArchived)
                .SortByDescending(i => i.InvoiceDate)
                .ToListAsync();

        public async Task<IEnumerable<InvoiceDocument>> GetByStatusAsync(string status) =>
            await _ctx.Invoices.Find(i => i.Status == status && !i.IsArchived)
                .SortByDescending(i => i.CreatedDate)
                .ToListAsync();

        public async Task<IEnumerable<InvoiceDocument>> GetOverdueAsync()
        {
            var today = DateTime.UtcNow.Date;
            var filter = Builders<InvoiceDocument>.Filter.And(
                Builders<InvoiceDocument>.Filter.Lt(i => i.DueDate, today),
                Builders<InvoiceDocument>.Filter.Nin(i => i.Status, new[] { InvoiceStatus.Paid, InvoiceStatus.Cancelled, InvoiceStatus.Archived }),
                Builders<InvoiceDocument>.Filter.Eq(i => i.IsArchived, false)
            );
            return await _ctx.Invoices.Find(filter).SortBy(i => i.DueDate).ToListAsync();
        }

        public async Task<IEnumerable<InvoiceDocument>> GetDueBetweenAsync(DateTime from, DateTime to) =>
            await _ctx.Invoices.Find(i => i.DueDate >= from && i.DueDate <= to && !i.IsArchived)
                .SortBy(i => i.DueDate)
                .ToListAsync();

        public async Task<InvoiceDocument> CreateAsync(InvoiceDocument invoice)
        {
            await _ctx.Invoices.InsertOneAsync(invoice);
            return invoice;
        }

        public async Task<InvoiceDocument> UpdateAsync(InvoiceDocument invoice)
        {
            invoice.ModifiedDate = DateTime.UtcNow;
            await _ctx.Invoices.ReplaceOneAsync(i => i.Id == invoice.Id, invoice);
            return invoice;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _ctx.Invoices.DeleteOneAsync(i => i.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ArchiveAsync(string id)
        {
            var update = Builders<InvoiceDocument>.Update
                .Set(i => i.IsArchived, true)
                .Set(i => i.Status, InvoiceStatus.Archived)
                .Set(i => i.ModifiedDate, DateTime.UtcNow);
            var result = await _ctx.Invoices.UpdateOneAsync(i => i.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            // Atomic findOneAndUpdate to increment sequence counter
            var currentYear = DateTime.UtcNow.Year;
            var filter = Builders<InvoiceSequenceDocument>.Filter.Eq(s => s.Id, "invoice_sequence");
            var update = Builders<InvoiceSequenceDocument>.Update
                .Inc(s => s.LastSequenceNumber, 1)
                .Set(s => s.Year, currentYear);
            var options = new FindOneAndUpdateOptions<InvoiceSequenceDocument>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            var seq = await _ctx.Sequences.FindOneAndUpdateAsync(filter, update, options);
            return $"INV-{seq.Year}-{seq.LastSequenceNumber:D5}";
        }

        public async Task<long> CountByStatusAsync(string status) =>
            await _ctx.Invoices.CountDocumentsAsync(i => i.Status == status && !i.IsArchived);
    }

    // ══════════════════════════════════════════════════════════════
    //  Customer Repository
    // ══════════════════════════════════════════════════════════════
    public class CustomerRepository : ICustomerRepository
    {
        private readonly MongoDbContext _ctx;
        public CustomerRepository(MongoDbContext ctx) => _ctx = ctx;

        public async Task<CustomerDocument?> GetByIdAsync(string id) =>
            await _ctx.Customers.Find(c => c.Id == id).FirstOrDefaultAsync();

        public async Task<IEnumerable<CustomerDocument>> GetAllAsync(bool activeOnly = true)
        {
            var filter = activeOnly
                ? Builders<CustomerDocument>.Filter.Eq(c => c.IsActive, true)
                : Builders<CustomerDocument>.Filter.Empty;
            return await _ctx.Customers.Find(filter).SortBy(c => c.CustomerName).ToListAsync();
        }

        public async Task<IEnumerable<CustomerDocument>> SearchByNameAsync(string name)
        {
            var filter = Builders<CustomerDocument>.Filter.Regex(c => c.CustomerName,
                new MongoDB.Bson.BsonRegularExpression(name, "i"));
            return await _ctx.Customers.Find(filter).ToListAsync();
        }

        public async Task<CustomerDocument> CreateAsync(CustomerDocument customer)
        {
            await _ctx.Customers.InsertOneAsync(customer);
            return customer;
        }

        public async Task<CustomerDocument> UpdateAsync(CustomerDocument customer)
        {
            customer.ModifiedDate = DateTime.UtcNow;
            await _ctx.Customers.ReplaceOneAsync(c => c.Id == customer.Id, customer);
            return customer;
        }

        public async Task<bool> SoftDeleteAsync(string id)
        {
            var update = Builders<CustomerDocument>.Update
                .Set(c => c.IsActive, false)
                .Set(c => c.ModifiedDate, DateTime.UtcNow);
            var result = await _ctx.Customers.UpdateOneAsync(c => c.Id == id, update);
            return result.ModifiedCount > 0;
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  Payment Repository
    // ══════════════════════════════════════════════════════════════
    public class PaymentRepository : IPaymentRepository
    {
        private readonly MongoDbContext _ctx;
        public PaymentRepository(MongoDbContext ctx) => _ctx = ctx;

        public async Task<PaymentDocument?> GetByIdAsync(string id) =>
            await _ctx.Payments.Find(p => p.Id == id).FirstOrDefaultAsync();

        public async Task<IEnumerable<PaymentDocument>> GetByInvoiceIdAsync(string invoiceId) =>
            await _ctx.Payments.Find(p => p.InvoiceId == invoiceId && !p.IsReversed)
                .SortByDescending(p => p.PaymentDate).ToListAsync();

        public async Task<IEnumerable<PaymentDocument>> GetByCustomerIdAsync(string customerId) =>
            await _ctx.Payments.Find(p => p.CustomerId == customerId && !p.IsReversed)
                .SortByDescending(p => p.PaymentDate).ToListAsync();

        public async Task<IEnumerable<PaymentDocument>> GetByDateRangeAsync(DateTime from, DateTime to) =>
            await _ctx.Payments.Find(p => p.PaymentDate >= from && p.PaymentDate <= to && !p.IsReversed)
                .SortByDescending(p => p.PaymentDate).ToListAsync();

        public async Task<PaymentDocument> CreateAsync(PaymentDocument payment)
        {
            await _ctx.Payments.InsertOneAsync(payment);
            return payment;
        }

        public async Task<bool> MarkReversedAsync(string id, string reason)
        {
            var update = Builders<PaymentDocument>.Update
                .Set(p => p.IsReversed, true)
                .Set(p => p.ReversalReason, reason);
            var result = await _ctx.Payments.UpdateOneAsync(p => p.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<decimal> GetTotalPaidForInvoiceAsync(string invoiceId)
        {
            var payments = await _ctx.Payments
                .Find(p => p.InvoiceId == invoiceId && !p.IsReversed)
                .ToListAsync();
            decimal total = 0;
            foreach (var p in payments) total += p.PaymentAmount;
            return total;
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  Payment Method Repository
    // ══════════════════════════════════════════════════════════════
    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly MongoDbContext _ctx;
        public PaymentMethodRepository(MongoDbContext ctx) => _ctx = ctx;

        public async Task<IEnumerable<PaymentMethodDocument>> GetAllActiveAsync() =>
            await _ctx.PaymentMethods.Find(m => m.IsActive).SortBy(m => m.MethodName).ToListAsync();

        public async Task<PaymentMethodDocument?> GetByIdAsync(string id) =>
            await _ctx.PaymentMethods.Find(m => m.Id == id).FirstOrDefaultAsync();

        public async Task<PaymentMethodDocument> CreateAsync(PaymentMethodDocument method)
        {
            await _ctx.PaymentMethods.InsertOneAsync(method);
            return method;
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  Audit Repository
    // ══════════════════════════════════════════════════════════════
    public class AuditRepository : IAuditRepository
    {
        private readonly MongoDbContext _ctx;
        public AuditRepository(MongoDbContext ctx) => _ctx = ctx;

        public async Task LogAsync(AuditLogDocument log) =>
            await _ctx.AuditLogs.InsertOneAsync(log);

        public async Task<IEnumerable<AuditLogDocument>> GetByInvoiceIdAsync(string invoiceId) =>
            await _ctx.AuditLogs.Find(a => a.InvoiceId == invoiceId)
                .SortByDescending(a => a.Timestamp).ToListAsync();

        public async Task<IEnumerable<AuditLogDocument>> GetByEventTypeAsync(
            string eventType, DateTime? from = null, DateTime? to = null)
        {
            var filter = Builders<AuditLogDocument>.Filter.Eq(a => a.EventType, eventType);
            if (from.HasValue) filter &= Builders<AuditLogDocument>.Filter.Gte(a => a.Timestamp, from.Value);
            if (to.HasValue) filter &= Builders<AuditLogDocument>.Filter.Lte(a => a.Timestamp, to.Value);
            return await _ctx.AuditLogs.Find(filter).SortByDescending(a => a.Timestamp).ToListAsync();
        }

        public async Task<IEnumerable<AuditLogDocument>> GetRecentAsync(int count = 50) =>
            await _ctx.AuditLogs.Find(_ => true)
                .SortByDescending(a => a.Timestamp).Limit(count).ToListAsync();
    }

    // ══════════════════════════════════════════════════════════════
    //  Reconciliation Repository
    // ══════════════════════════════════════════════════════════════
    public class ReconciliationRepository : IReconciliationRepository
    {
        private readonly MongoDbContext _ctx;
        public ReconciliationRepository(MongoDbContext ctx) => _ctx = ctx;

        public async Task CreateAsync(ReconciliationDocument record) =>
            await _ctx.Reconciliations.InsertOneAsync(record);

        public async Task<IEnumerable<ReconciliationDocument>> GetByInvoiceIdAsync(string invoiceId) =>
            await _ctx.Reconciliations.Find(r => r.InvoiceId == invoiceId)
                .SortByDescending(r => r.ReconciledAt).ToListAsync();

        public async Task<IEnumerable<ReconciliationDocument>> GetByDateRangeAsync(DateTime from, DateTime to) =>
            await _ctx.Reconciliations.Find(r => r.PaymentDate >= from && r.PaymentDate <= to)
                .SortByDescending(r => r.PaymentDate).ToListAsync();
    }

    // ══════════════════════════════════════════════════════════════
    //  Analytics Repository
    // ══════════════════════════════════════════════════════════════
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly MongoDbContext _ctx;
        public AnalyticsRepository(MongoDbContext ctx) => _ctx = ctx;

        public async Task UpsertSnapshotAsync(AnalyticsSnapshotDocument snapshot)
        {
            var filter = Builders<AnalyticsSnapshotDocument>.Filter.Eq(s => s.Period, snapshot.Period);
            await _ctx.AnalyticsSnapshots.ReplaceOneAsync(filter, snapshot,
                new ReplaceOptions { IsUpsert = true });
        }

        public async Task<AnalyticsSnapshotDocument?> GetByPeriodAsync(string period) =>
            await _ctx.AnalyticsSnapshots.Find(s => s.Period == period).FirstOrDefaultAsync();

        public async Task<IEnumerable<AnalyticsSnapshotDocument>> GetByYearAsync(int year) =>
            await _ctx.AnalyticsSnapshots.Find(s => s.Period.StartsWith(year.ToString()))
                .SortBy(s => s.Period).ToListAsync();
    }
}
