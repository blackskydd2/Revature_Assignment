using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoiceManagement.DAL.Repositories.Interfaces
{
    /// <summary>
    /// Generic repository interface providing standard CRUD operations.
    /// All entity-specific repositories extend this.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(string id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}