using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models.DTOs
{
    public class CreateProjectDto
    {
        public Guid TeamLeadId { get; set; }
        public string ProjectName   { get; set; }
        public string? ProjectDescription { get; set; }

    }
}
