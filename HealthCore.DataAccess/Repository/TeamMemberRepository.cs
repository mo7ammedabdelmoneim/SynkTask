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
    public class TeamMemberRepository : GenericRepository<TeamMember>, ITeamMemberRepository
    {
        public TeamMemberRepository(
             ApplicationDbContext context,
             ILogger<GenericRepository<TeamMember>> logger) : base(context, logger)
        {
        }

        public async Task<TeamMember> GetTeamMemberWithTasksAsync(Guid teamMemberId)
        {
            return await context.TeamMembers.Include(m => m.ProjectTasks)
                                            .ThenInclude(t => t.Todos).
                                            FirstOrDefaultAsync(m => m.Id == teamMemberId);
        }

    }
}
