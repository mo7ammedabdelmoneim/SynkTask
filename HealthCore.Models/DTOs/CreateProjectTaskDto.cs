using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models.DTOs
{
    public class CreateProjectTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public Guid ProjectId { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public Priority Priority { get; set; }

        public Guid? AssignedMemberId { get; set; }


}
}
