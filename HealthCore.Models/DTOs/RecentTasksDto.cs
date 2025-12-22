using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models.DTOs
{
    public class RecentTasksDto
    {
        public string Title { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
