using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }


        public Guid TeamLeadId { get; set; }

        [ForeignKey(nameof(TeamLeadId))]
        public TeamLead TeamLead { get; set; }
        

        public IEnumerable<TeamMember> Members { get; set; } = new List<TeamMember>();
        public IEnumerable<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    }
}
