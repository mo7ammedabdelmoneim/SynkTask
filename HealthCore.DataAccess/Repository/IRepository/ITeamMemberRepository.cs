using SynkTask.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.DataAccess.Repository.IRepository
{
    public interface ITeamMemberRepository : IGenericRepository<TeamMember>
    {
        Task<TeamMember> GetTeamMemberWithTasksAsync(Guid teamMemberId);
    }
}
