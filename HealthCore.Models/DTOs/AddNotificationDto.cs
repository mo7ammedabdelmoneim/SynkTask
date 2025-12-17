using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models.DTOs
{
    public class AddNotificationDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Role { get; set; }
        public Guid UserId { get; set; }
    }
}
