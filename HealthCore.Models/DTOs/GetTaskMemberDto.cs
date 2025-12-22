using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models.DTOs
{
    public class GetTaskMemberDto
    {
        public Guid MemberId { get; set; }
        public string MemberName { get; set; }
        public string Email {  get; set; }
        public string ImageUrl { get; set; }
    }
}
