using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models
{
    public class IdentityApplicationUser
    {
        [Key]
        public string IdentityUserId { get; set; }
        public Guid ApplicationUserId { get; set; }
    }
}
