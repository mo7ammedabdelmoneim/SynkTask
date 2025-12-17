using SynkTask.DataAccess.IConfiguration;
using SynkTask.DataAccess.Repository;
using SynkTask.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynkTask.DataAccess.Repository.IRepository;

namespace SynkTask.DataAccess.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext context;
        private readonly ILoggerFactory loggerFactory;

        public IUserRepository Users { get;}
        public ICountryRepository Countries { get;}
        public INotificationRepository Notifications { get; }
        public ITeamLeadRepository TeamLeads { get; }
        public ITeamMemberRepository TeamMembers { get; }
        public IIdentityApplicationUserRepository IdentityApplicationUsers { get; }


        public UnitOfWork(ApplicationDbContext context, ILoggerFactory loggerFactory)
        {
            this.context = context;
            this.loggerFactory = loggerFactory;

            Users = new UserRepository(
                context,
                loggerFactory.CreateLogger<GenericRepository<ApplicationUser>>()
            );
            Countries = new CountryRepository(
                context,
                loggerFactory.CreateLogger<GenericRepository<Country>>()
            );
            Notifications = new NotificationRepository(
                context,
                loggerFactory.CreateLogger<GenericRepository<Notification>>()
            );
            TeamMembers = new TeamMemberRepository(
                context,
                loggerFactory.CreateLogger<GenericRepository<TeamMember>>()
            );
            TeamLeads = new TeamLeadRepository(
                context,
                loggerFactory.CreateLogger<GenericRepository<TeamLead>>()
            );
            IdentityApplicationUsers = new IdentityApplicationUserRepository(
                context,
                loggerFactory.CreateLogger<GenericRepository<IdentityApplicationUser>>()
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
