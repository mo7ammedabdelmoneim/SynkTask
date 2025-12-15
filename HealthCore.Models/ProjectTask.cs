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

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsCompleted { get; set; }
        public Priority Priority { get; set; }

        public Guid AssignedMemberId { get; set; }

        [ForeignKey(nameof(AssignedMemberId))]
        public TeamMember AssignedMember { get; set; }


        public IEnumerable<Todo> Todos { get; set; } = new List<Todo>();
    }

    public enum Priority
    {
        High,
        Medium,
        Low,
    }
}
