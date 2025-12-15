using SynkTask.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<ApplicationUser> Users {  get; set; }
        public virtual DbSet<Country> Countries {  get; set; }
        public virtual DbSet<TeamLead> TeamLeads {  get; set; }
        public virtual DbSet<TeamMember> TeamMembers {  get; set; }
        public virtual DbSet<Project> Projects  {  get; set; }
        public virtual DbSet<ProjectTask> ProjectTasks {  get; set; }
        public virtual DbSet<Todo> Todos {  get; set; }
        public virtual DbSet<Notification> Notifications {  get; set; }
    }
}
