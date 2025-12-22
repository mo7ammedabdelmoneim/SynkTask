using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models.DTOs
{
    public class GetTeamMemberOfTeamLeadResponseDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? ImageUrl { get; set; }

        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int ComplttedTasks { get; set; }
    }
}
