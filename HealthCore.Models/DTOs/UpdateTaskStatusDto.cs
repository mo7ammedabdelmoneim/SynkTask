using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models.DTOs
{
    public class UpdateTaskStatusDto
    {
        public Guid TaskId { get; set; }
        public string Status { get; set; }
    }
}
