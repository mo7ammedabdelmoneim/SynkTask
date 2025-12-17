using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models
{
    public class IdentityApplicationUser
    {
        public string IdentityUserId { get; set; }
        public Guid ApplicationUserId { get; set; }
    }
}
