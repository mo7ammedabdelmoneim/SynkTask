using SynkTask.DataAccess.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynkTask.DataAccess.Repository.IRepository;
using SynkTask.Models.Models;

namespace SynkTask.DataAccess.Repository
{
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        public ProjectRepository(
             ApplicationDbContext context,
             ILogger<GenericRepository<Project>> logger) : base(context, logger)
        {
        }

    }
}
