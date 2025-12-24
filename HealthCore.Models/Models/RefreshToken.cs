using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynkTask.Models.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public string Token { get; set; }
        public DateTime ExpiresOn { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
        public DateTime CreatedOn { get; set; }
        public DateTime? RevokedOn { get; set; }

        public bool IsActive => RevokedOn == null && !IsExpired;


        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

    }
}
