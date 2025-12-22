using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models.DTOs
{
    public class GetTeamLeadTaskInfoResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public DateTime FromDate { get; set; } 
        public DateTime DueDate { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }

        public int Todos { get; set; }
        public int CompletedTodos { get; set; }


        public IEnumerable<string> AssignedMembersPicture { get; set; } = new List<string>();
    }
}
