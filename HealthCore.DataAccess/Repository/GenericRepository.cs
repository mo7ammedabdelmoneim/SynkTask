using SynkTask.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SynkTask.DataAccess.Repository.IRepository;

namespace SynkTask.DataAccess.Repository
{
    public class GenericRepository<T> : IGenericRepository<T>
    where T : class, new()
    {
        protected readonly ApplicationDbContext context;
        private readonly ILogger<GenericRepository<T>> logger;
        internal DbSet<T> dbSet;

        public GenericRepository(ApplicationDbContext context, ILogger<GenericRepository<T>> logger)
        {
            this.context = context;
            this.logger = logger;
            dbSet = context.Set<T>();
        }

        public virtual async Task<bool> AddAsync(T entity)
        {
            try
            {
                await dbSet.AddAsync(entity);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"AddAsync Error in {typeof(T).Name}");
                return false;
            }
        }

        public bool Delete(T entity)
        {
            try
            {
                dbSet.Remove(entity);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Delete Error in {typeof(T).Name}");
                return false;
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            string? includedProperties = null)
        {
            try
            {
                IQueryable<T> query = dbSet;

                if (filter != null)
                    query = query.Where(filter);

                if (!string.IsNullOrEmpty(includedProperties))
                {
                    var properties = includedProperties.Split(',');
                    foreach (var prop in properties)
                        query = query.Include(prop.Trim());
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"GetAllAsync Error in {typeof(T).Name}");
                return Enumerable.Empty<T>();
            }
        }

        public virtual async Task<T?> GetAsync( Expression<Func<T, bool>> filter,  string? includedProperties = null)
        {
            try
            {
                IQueryable<T> query = dbSet;

                if (!string.IsNullOrEmpty(includedProperties))
                {
                    var properties = includedProperties.Split(',');
                    foreach (var prop in properties)
                        query = query.Include(prop.Trim());
                }

                query = query.Where(filter);

                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"GetAsync Error in {typeof(T).Name}");
                return new T();
            }
        }

        public virtual bool Update(T entity)
        {
            try
            {
                dbSet.Update(entity);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Update Error in {typeof(T).Name}");
                return false;
            }
        }
    }
}
