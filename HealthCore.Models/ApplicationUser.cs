using Microsoft.AspNetCore.Identity;

namespace SynkTask.Models
{
    public class ApplicationUser:IdentityUser
    {
        public List<RefreshToken> RefreshTokens { get; set; }

    }
}
