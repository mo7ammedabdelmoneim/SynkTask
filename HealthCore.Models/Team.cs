using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models
{
    public class Team
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string TeamIdentifier { get; set; }
        public IEnumerable<TeamMember> Members { get; set; } = new List<TeamMember>();

        public Guid? TeamLeadId { get; set; }

        [ForeignKey(nameof(TeamLeadId))]
        public TeamLead TeamLead { get; set; }

    }
}
