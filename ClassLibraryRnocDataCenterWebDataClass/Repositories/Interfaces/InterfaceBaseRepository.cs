using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces
{


    public interface InterfaceBaseRepository<T> where T : class
    {
        // Basic CRUD operations
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByIdAsync(long id);
        Task<T?> GetByIdAsync(object id); // ← THÊM DÒNG NÀY
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(); // ← THÊM DÒNG NÀY
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate); // ← THÊM DÒNG NÀY

        // Add/Update/Delete
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task UpdateRangeAsync(IEnumerable<T> entities);
        Task DeleteAsync(T entity);
        Task DeleteAsync(int id);
        Task DeleteAsync(long id);
        Task<bool> DeleteAsync(object id); // ← THÊM DÒNG NÀY

        Task DeleteRangeAsync(IEnumerable<T> entities);

        // Advanced queries
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<T>> GetPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize);

        // Save changes
        Task<int> SaveChangesAsync();
    }
}
