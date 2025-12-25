using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models.DTOs
{
    public class GetTaskInfoResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid ProjectId { get; set; }
        public Guid TeamLeadId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }

        public List<string> AssignedMemebersPicture { get; set; } = new List<string>();
        public List<GetTodoInfoResponseDto> Todos { get; set; } = new List<GetTodoInfoResponseDto>();
        public List<TaskAttachmentDto> Attachments { get; set; } = new List<TaskAttachmentDto>();


    }
}
