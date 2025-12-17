using SynkTask.DataAccess.Repository;
using SynkTask.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.DataAccess.IConfiguration
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        ICountryRepository Countries { get; }
        INotificationRepository Notifications { get; }
        ITeamLeadRepository TeamLeads { get; }
        ITeamMemberRepository TeamMembers { get; }
        IIdentityApplicationUserRepository IdentityApplicationUsers { get; }


        Task CompleteAsync();
    }
}
