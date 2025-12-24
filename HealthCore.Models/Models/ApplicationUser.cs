using Microsoft.AspNetCore.Identity;

namespace SynkTask.Models.Models
{
    public class ApplicationUser : IdentityUser
    {
        public List<RefreshToken> RefreshTokens { get; set; }

    }
}
