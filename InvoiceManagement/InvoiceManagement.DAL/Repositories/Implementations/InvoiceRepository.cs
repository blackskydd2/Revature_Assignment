using MongoDB.Driver;
using InvoiceManagement.DAL.Context;
using InvoiceManagement.DAL.Models;
using InvoiceManagement.DAL.Repositories.Interfaces;

namespace InvoiceManagement.DAL.Repositories.Implementations
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly IMongoCollection<Invoice> _collection;

        public InvoiceRepository(AppDbContext context)
        {
            _collection = context.Invoices;
        }

        public async Task AddAsync(Invoice invoice)
        {
            await _collection.InsertOneAsync(invoice);
        }

        public async Task<List<Invoice>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<Invoice> GetByIdAsync(string id)
        {
            return await _collection
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();
        }
    }
}