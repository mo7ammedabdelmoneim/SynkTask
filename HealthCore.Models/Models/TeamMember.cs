using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models.Models
{
    public class TeamMember
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        //public string Role { get; set; } = "TEAMMEMBER";
        public string? ImageUrl { get; set; }



        public string ApplicationUserId { get; set; }

        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser ApplicationUser { get; set; }

        public Guid? TeamId { get; set; }

        [ForeignKey(nameof(TeamId))]
        public Team Team { get; set; }

        public IEnumerable<ProjectTask> ProjectTasks { get; set; } = new List<ProjectTask>();
    }
}
