using SynkTask.DataAccess.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynkTask.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using SynkTask.Models.Models;

namespace SynkTask.DataAccess.Repository
{
    public class ProjectTaskRepository : GenericRepository<ProjectTask>, IProjectTaskRepository
    {
        public ProjectTaskRepository(
             ApplicationDbContext context,
             ILogger<GenericRepository<ProjectTask>> logger) : base(context, logger)
        {
        }

        public async Task ResetAssignedMembersAsync(Guid taskId)
        {
            var task = await context.ProjectTasks.Include(t=>t.AssignedMembers).FirstOrDefaultAsync(t=>t.Id == taskId);

            task?.AssignedMembers.Clear();
            context.SaveChanges();
        }

    }
}
