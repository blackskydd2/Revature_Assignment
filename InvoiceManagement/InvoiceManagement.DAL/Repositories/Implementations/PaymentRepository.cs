using InvoiceManagement.DAL.Models;
using InvoiceManagement.DAL.Repositories.Interfaces;
using MongoDB.Driver;

namespace InvoiceManagement.DAL.Repositories.Implementations
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IMongoCollection<Payment> _collection;

        public PaymentRepository()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("InvoiceDb");
            _collection = database.GetCollection<Payment>("Payments");
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _collection.Find(_ => true)
                                    .SortByDescending(p => p.PaymentDate)
                                    .ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(string id)
        {
            return await _collection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Payment> AddAsync(Payment payment)
        {
            await _collection.InsertOneAsync(payment);
            return payment;
        }

        public async Task<Payment> UpdateAsync(Payment payment)
        {
            await _collection.ReplaceOneAsync(p => p.Id == payment.Id, payment);
            return payment;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _collection.Find(p => p.Id == id).AnyAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByInvoiceIdAsync(string invoiceId)
        {
            return await _collection.Find(p => p.InvoiceId == invoiceId)
                                    .SortBy(p => p.PaymentDate)
                                    .ToListAsync();
        }

        public async Task<decimal> GetTotalPaidForInvoiceAsync(string invoiceId)
        {
            var payments = await _collection.Find(p => p.InvoiceId == invoiceId).ToListAsync();
            return payments.Sum(p => p.PaymentAmount);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByMethodAsync(string methodId)
        {
            return await _collection.Find(p => p.MethodId == methodId).ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _collection.Find(p => p.PaymentDate >= from && p.PaymentDate <= to)
                                    .SortBy(p => p.PaymentDate)
                                    .ToListAsync();
        }
    }
}