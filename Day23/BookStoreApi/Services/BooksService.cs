using BookStoreApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BookStoreApi.Services
{
    public class BooksService
    {
        private readonly IMongoCollection<Books> _booksCollection;

        public BooksService(IMongoClient client, IOptions<BookstoreDatabaseSettings> settings)
        {
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _booksCollection = database.GetCollection<Books>(settings.Value.BooksCollectionName);
        }

        public async Task<List<Books>> GetAsync() =>
            await _booksCollection.Find(_ => true).ToListAsync();

        public async Task<Books?> GetAsync(string id) =>
            await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Books newBook) =>
            await _booksCollection.InsertOneAsync(newBook);

        public async Task UpdateAsync(string id, Books updatedBook) =>
            await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

        public async Task RemoveAsync(string id) =>
            await _booksCollection.DeleteOneAsync(x => x.Id == id);
    }
}