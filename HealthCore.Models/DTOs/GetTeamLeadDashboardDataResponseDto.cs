using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models.DTOs
{
    public class GetTeamLeadDashboardDataResponseDto
    {
        // Task Status
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }

        // Task Priority
        public int LowTasks { get; set; }
        public int MediumTasks { get; set; }
        public int HighTasks { get; set; }


        // Recent Tasks
        public List<RecentTasksDto> RecentTasks { get; set; } = new List<RecentTasksDto>();
    }
}
