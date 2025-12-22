using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models
{
    public class ProjectTask
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public Guid ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public Project Project { get; set; }

        public Guid TeamLeadId { get; set; }
        
        [ForeignKey(nameof(TeamLeadId))]
        public TeamLead TeamLead { get; set; }

        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }

        public ICollection<TeamMember> AssignedMembers { get; set; } = new List<TeamMember>();  
        public ICollection<Todo> Todos { get; set; } = new List<Todo>();
    }
}
