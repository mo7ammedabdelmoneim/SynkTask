using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.DataAccess.Repository.IRepository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includedProperties = null);
        Task<T> GetAsync(Expression<Func<T, bool>>? filter = null, string? includedProperties = null);
        Task<bool> AddAsync(T entity);
        bool Delete(T entity);
        bool Update(T entity);
    }
}
