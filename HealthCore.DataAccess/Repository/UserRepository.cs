using HealthCore.DataAccess.Data;
using HealthCore.DataAccess.IRepository;
using HealthCore.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCore.DataAccess.Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(
             ApplicationDbContext context,
             ILogger<GenericRepository<User>> logger) : base(context, logger)
        {
        }

    }
}
