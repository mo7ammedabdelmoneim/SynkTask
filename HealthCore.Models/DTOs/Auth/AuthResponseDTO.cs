using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SynkTask.Models.DTOs.Auth
{
    public class AuthResponseDTO 
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IList<string>? Roles { get; set; }

        [JsonIgnore]
        public string? RefreshToken { get; set; }

        public DateTime RefreshTokenExpiration { get; set; }

    }
}
