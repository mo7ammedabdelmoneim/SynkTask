using HealthCore.DataAccess.IConfiguration;
using HealthCore.DataAccess.IRepository;
using HealthCore.DataAccess.Repository;
using HealthCore.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCore.DataAccess.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext context;
        private readonly ILoggerFactory loggerFactory;

        public IUserRepository Users { get;}

        public UnitOfWork(ApplicationDbContext context, ILoggerFactory loggerFactory)
        {
            this.context = context;
            this.loggerFactory = loggerFactory;

            Users = new UserRepository(
                context,
                loggerFactory.CreateLogger<GenericRepository<User>>()
            );
        }


        public async Task CompleteAsync()
        {
            await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
