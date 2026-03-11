using MongoDB.Driver;
using InvoiceManagement.DAL.Models;

namespace InvoiceManagement.DAL.Context
{
    public class AppDbContext
    {
        private readonly IMongoDatabase _database;

        public AppDbContext()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            _database = client.GetDatabase("InvoiceManagementDb");
        }

        public IMongoCollection<Invoice> Invoices =>
            _database.GetCollection<Invoice>("Invoices");

        public IMongoCollection<Payment> Payments =>
            _database.GetCollection<Payment>("Payments");

        public IMongoCollection<PaymentMethod> PaymentMethods =>
            _database.GetCollection<PaymentMethod>("PaymentMethods");
    }
}