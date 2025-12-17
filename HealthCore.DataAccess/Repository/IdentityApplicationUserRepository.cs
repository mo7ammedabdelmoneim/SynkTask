using SynkTask.DataAccess.Data;
using SynkTask.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynkTask.DataAccess.Repository.IRepository;

namespace SynkTask.DataAccess.Repository
{
    public class IdentityApplicationUserRepository : GenericRepository<IdentityApplicationUser>, IIdentityApplicationUserRepository
    {
        public IdentityApplicationUserRepository(
             ApplicationDbContext context,
             ILogger<GenericRepository<IdentityApplicationUser>> logger) : base(context, logger)
        {
        }

    }
}
