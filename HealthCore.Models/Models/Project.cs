using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models.Models
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;


        public Guid TeamLeadId { get; set; }

        [ForeignKey(nameof(TeamLeadId))]
        public TeamLead TeamLead { get; set; }

        public IEnumerable<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    }
}
