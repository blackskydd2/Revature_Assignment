using InvoiceManagement.DAL.Documents;
using MongoDB.Driver;

namespace InvoiceManagement.DAL.Context
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "InvoiceManagementDB";
    }

    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
            EnsureIndexes();
        }

        // ─── Collections ────────────────────────────────────────────
        public IMongoCollection<InvoiceDocument> Invoices =>
            _database.GetCollection<InvoiceDocument>("Invoices");

        public IMongoCollection<CustomerDocument> Customers =>
            _database.GetCollection<CustomerDocument>("Customers");

        public IMongoCollection<PaymentDocument> Payments =>
            _database.GetCollection<PaymentDocument>("Payments");

        public IMongoCollection<PaymentMethodDocument> PaymentMethods =>
            _database.GetCollection<PaymentMethodDocument>("PaymentMethods");

        public IMongoCollection<InvoiceSequenceDocument> Sequences =>
            _database.GetCollection<InvoiceSequenceDocument>("Sequences");

        public IMongoCollection<AuditLogDocument> AuditLogs =>
            _database.GetCollection<AuditLogDocument>("AuditLogs");

        public IMongoCollection<ReconciliationDocument> Reconciliations =>
            _database.GetCollection<ReconciliationDocument>("Reconciliations");

        public IMongoCollection<AnalyticsSnapshotDocument> AnalyticsSnapshots =>
            _database.GetCollection<AnalyticsSnapshotDocument>("AnalyticsSnapshots");

        // ─── Index Setup ─────────────────────────────────────────────
        private void EnsureIndexes()
        {
            // Invoices: unique invoice number, query by customer/status/dueDate
            var invIdx = Builders<InvoiceDocument>.IndexKeys;
            Invoices.Indexes.CreateOne(new CreateIndexModel<InvoiceDocument>(
                invIdx.Ascending(i => i.InvoiceNumber),
                new CreateIndexOptions { Unique = true }));
            Invoices.Indexes.CreateOne(new CreateIndexModel<InvoiceDocument>(
                invIdx.Ascending(i => i.CustomerId)));
            Invoices.Indexes.CreateOne(new CreateIndexModel<InvoiceDocument>(
                invIdx.Ascending(i => i.Status)));
            Invoices.Indexes.CreateOne(new CreateIndexModel<InvoiceDocument>(
                invIdx.Ascending(i => i.DueDate)));
            Invoices.Indexes.CreateOne(new CreateIndexModel<InvoiceDocument>(
                invIdx.Ascending(i => i.IsArchived)));

            // Payments: query by invoiceId, customerId, date
            var payIdx = Builders<PaymentDocument>.IndexKeys;
            Payments.Indexes.CreateOne(new CreateIndexModel<PaymentDocument>(
                payIdx.Ascending(p => p.InvoiceId)));
            Payments.Indexes.CreateOne(new CreateIndexModel<PaymentDocument>(
                payIdx.Ascending(p => p.CustomerId)));
            Payments.Indexes.CreateOne(new CreateIndexModel<PaymentDocument>(
                payIdx.Descending(p => p.PaymentDate)));

            // Customers
            var custIdx = Builders<CustomerDocument>.IndexKeys;
            Customers.Indexes.CreateOne(new CreateIndexModel<CustomerDocument>(
                custIdx.Text(c => c.CustomerName)));

            // Audit logs: by invoiceId and timestamp
            var auditIdx = Builders<AuditLogDocument>.IndexKeys;
            AuditLogs.Indexes.CreateOne(new CreateIndexModel<AuditLogDocument>(
                auditIdx.Ascending(a => a.InvoiceId)));
            AuditLogs.Indexes.CreateOne(new CreateIndexModel<AuditLogDocument>(
                auditIdx.Descending(a => a.Timestamp)));

            // Reconciliation: by invoiceId
            var reconIdx = Builders<ReconciliationDocument>.IndexKeys;
            Reconciliations.Indexes.CreateOne(new CreateIndexModel<ReconciliationDocument>(
                reconIdx.Ascending(r => r.InvoiceId)));

            // Analytics: unique period
            var snapIdx = Builders<AnalyticsSnapshotDocument>.IndexKeys;
            AnalyticsSnapshots.Indexes.CreateOne(new CreateIndexModel<AnalyticsSnapshotDocument>(
                snapIdx.Ascending(s => s.Period),
                new CreateIndexOptions { Unique = true }));
        }
    }
}
