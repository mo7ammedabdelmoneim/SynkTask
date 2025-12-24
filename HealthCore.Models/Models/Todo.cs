using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models.Models
{
    public class Todo
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public DateTime Created { get; set; } = DateTime.Now;
        public bool IsCompleted { get; set; } = false;



        public Guid TaskId { get; set; }

        [ForeignKey(nameof(TaskId))]
        public ProjectTask ProjectTask { get; set; }

        public Guid TeamMemberId { get; set; }

        [ForeignKey(nameof(TeamMemberId))]
        public TeamMember TeamMember { get; set; }

    }
}
