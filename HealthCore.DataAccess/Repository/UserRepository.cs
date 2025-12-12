using SynkTask.DataAccess.Data;
using SynkTask.DataAccess.IRepository;
using SynkTask.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.DataAccess.Repository
{
    public class UserRepository : GenericRepository<ApplicationUser>, IUserRepository
    {
        public UserRepository(
             ApplicationDbContext context,
             ILogger<GenericRepository<ApplicationUser>> logger) : base(context, logger)
        {
        }

    }
}
