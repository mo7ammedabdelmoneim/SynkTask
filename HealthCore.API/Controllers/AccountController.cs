using SynkTask.API.Configurations.Models;
using SynkTask.DataAccess.IConfiguration;
using SynkTask.DataAccess.Repository;
using SynkTask.Models;
using SynkTask.Models.DTOs.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SynkTask.Models.DTOs;
using Azure;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using SynkTask.Models.Models;

namespace SynkTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;
        private readonly JwtConfig jwtConfig;

        public AccountController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IOptionsMonitor<JwtConfig> optionsMonitor, IConfiguration configuration)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.configuration = configuration;
            jwtConfig = optionsMonitor.CurrentValue;
        }

        [HttpPost("Register")]
        [ProducesResponseType(typeof(AuthResponseDTO), 200)]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            var response = new ApiResponse<AuthResponseDTO>();

            if (!ModelState.IsValid)
            {
                response.Message = "Invalid input data";
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(response);
            }

            var user = await userManager.FindByEmailAsync(registerDTO.Email);
            if (user != null)
            {
                response.Message = "Invalid input data";
                response.Errors = new List<string> { "Email already exists" };
                return BadRequest(response);
            }

            var team = await unitOfWork.Teams.GetAsync(t => t.TeamIdentifier == registerDTO.TeamIdentifier);
            if (team == null)
            {
                response.Message = "Invalid input data";
                response.Errors = new List<string> { "Invalid TeamIdentifier" };
                return BadRequest(response);
            }

            var newUser = new ApplicationUser
            {
                UserName = $"{registerDTO.FirstName}.{registerDTO.LastName}",
                Email = registerDTO.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newUser, registerDTO.Password);
            if (!result.Succeeded)
            {
                response.Message = "Registration failed";
                response.Errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(response);
            }

            var roleResult = await userManager.AddToRoleAsync(newUser, "teamMember");
            if (!roleResult.Succeeded)
            {
                response.Message = "Registration failed";
                response.Errors = roleResult.Errors.Select(e => e.Description).ToList();
                return BadRequest(response);
            }

            var member = new TeamMember
            {
                FirstName = registerDTO.FirstName,
                LastName = registerDTO.LastName,
                Email = registerDTO.Email,
                Country = registerDTO.Country,
                TeamId = team.Id,
                ApplicationUserId = newUser.Id
            };
            await unitOfWork.TeamMembers.AddAsync(member);
            await unitOfWork.CompleteAsync();

            var identityApplicationUser = new IdentityApplicationUser
            {
                ApplicationUserId = member.Id,
                IdentityUserId = newUser.Id,
            };
            await unitOfWork.IdentityApplicationUsers.AddAsync(identityApplicationUser);
            await unitOfWork.CompleteAsync();

            response.Success = true;
            response.Message = "User registered successfully";
            var data = await CreateTokenAsync(newUser.Id,member.Id);
            response.Data = data;
            if (!string.IsNullOrEmpty(data.RefreshToken))
                SetRefreshTokenInCookie(data.RefreshToken, data.RefreshTokenExpiration);
            return Ok(response);
        }


        [HttpPost("profileImage/{userId}")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> UploadImage(Guid userId, IFormFile image)
        {
            var response = new ApiResponse<string>() { Message = "Invalid input data" };

            var user = await unitOfWork.TeamMembers.GetAsync(m => m.Id == userId);
            if(user == null)
            {
                response.Errors = new List<string> { "Invalid UserId" };
                return BadRequest(response);
            }

            List<string> allowedExtensions = new List<string>() { ".png", ".jpg", ".jpeg" };
            const int maxSize = 2 * 1024 * 1024;

            var extension = Path.GetExtension(image.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                response.Errors = new List<string> { "The extension of file is not valid, Just png, jpg and jpeg are allowed" };
                return BadRequest(response);
            }

            if (image.Length > maxSize || image.Length == 0)
            {
                response.Errors = new List<string> { $"The size of file is too large, {maxSize} is the maximum size" };
                return BadRequest(response);
            }
        
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "userProfileImages");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var saveFileName = Path.GetFileNameWithoutExtension(image.FileName);
            var fileName = $"{Guid.NewGuid()}_{saveFileName}{extension}";

            var filePath = Path.Combine(folderPath, fileName);

            using FileStream fs = new FileStream(filePath, FileMode.Create);
            image.CopyTo(fs);
            var imageUrl = $"userProfileImages/{fileName}";

            user.ImageUrl = imageUrl;
            await unitOfWork.CompleteAsync();

            response.Success = true;
            response.Message = "Image Uploaded Successfully";
            response.Data = $"ImageUrl : {imageUrl}";
            return Ok(response);
        }

        [HttpPost("Login")]
        [ProducesResponseType(typeof(AuthResponseDTO), 200)]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            var response = new ApiResponse<AuthResponseDTO>();

            if (!ModelState.IsValid)
            {
                response.Message = "Invalid input data";
                response.Errors = ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(response);
            }

            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                response.Message = "Verification Failed";
                response.Errors = new List<string>() { "Email Or Password is Wrong." };
                return BadRequest(response);
            }

            var check = await userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!check)
            {
                response.Message = "Verification Failed";
                response.Errors = new List<string>() { "Email Or Password is Wrong." };
                return BadRequest(response);
            }

            var appUser= await unitOfWork.IdentityApplicationUsers.GetAsync(u=> u.IdentityUserId == user.Id);
            Guid? appUserId = null;
            if (appUser != null)
                appUserId = appUser.ApplicationUserId;

            response.Success = true;
            response.Message = "User Login successfully";
            var data = await CreateTokenAsync(user.Id, appUser.ApplicationUserId);
            response.Data = data;
            if (!string.IsNullOrEmpty(data.RefreshToken))
                SetRefreshTokenInCookie(data.RefreshToken, data.RefreshTokenExpiration);
            return Ok(response);
        }

        [HttpGet("RefreshToken")]
        [ProducesResponseType(typeof(AuthResponseDTO), 200)] 
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (refreshToken == null)
                return BadRequest(new ApiResponse<string>
                { Message = "Operation Failed", Errors = new List<string> { "Refresh Token required In Cookie[refreshToken]" } });

            var refreshTokenResult = await RefreshTokenAsync(refreshToken);
            if (refreshTokenResult.Token == null)
                return BadRequest(new ApiResponse<string>
                { Success = false, Message = "Operation Failed", Errors = new List<string> { "Invalid Token" } });

            SetRefreshTokenInCookie(refreshTokenResult.RefreshToken, refreshTokenResult.RefreshTokenExpiration);

            var response = new ApiResponse<AuthResponseDTO>
            {
                Success = true,
                Message = "Refresh Token Operation is done successfully.",
                Data = refreshTokenResult
            };
            return Ok(response);
        }
        



        async Task<AuthResponseDTO> CreateTokenAsync(string userId, Guid? appUsrId =null)
        {
            var user = await unitOfWork.Users.GetAsync(u => u.Id == userId, includedProperties: "RefreshTokens");
            AuthResponseDTO response = new();
            SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecurityKey"]));
            SigningCredentials signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            if(appUsrId != null)
                claims.Add(new Claim("appUsrId", appUsrId.ToString()));

            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            var roles = await userManager.GetRolesAsync(user);
            foreach (var roleName in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
            }

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: configuration["JWT:VaildIssuer"], // Web API (Provider)
                audience: configuration["JWT:VaildAudience"], // Consumer (Angular)
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: signingCredentials
            );

            response = new AuthResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles,
                UserId = appUsrId,
            };

            if (user.RefreshTokens.Any(t => t.IsActive))
            {
                var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
                response.RefreshToken = activeRefreshToken?.Token;
                response.RefreshTokenExpiration = activeRefreshToken.ExpiresOn;
            }
            else
            {
                var refreshToken = GenerateRefreshToken();
                response.RefreshToken = refreshToken?.Token;
                response.RefreshTokenExpiration = refreshToken.ExpiresOn;
                user.RefreshTokens.Add(refreshToken);
                await userManager.UpdateAsync(user);
            }

            return response;
        }

        RefreshToken GenerateRefreshToken()
        {
            var random = new byte[32];

            using var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(random);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(random),
                CreatedOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(30),
            };
        }

        void SetRefreshTokenInCookie(string refreshToken, DateTime expiresOn)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Expires = expiresOn.ToLocalTime(),
            };

            Response.Cookies.Append("refreshToken", refreshToken, options);
        }

        async Task<AuthResponseDTO> RefreshTokenAsync(string token)
        {
            // check if this refreshToken is related to a user
            var user = await unitOfWork.Users.GetAsync(u => u.RefreshTokens.Any(t => t.Token == token), includedProperties: "RefreshTokens");
            if (user == null)
                return new AuthResponseDTO();

            // check if this refreshToken is still active
            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);
            if (!refreshToken.IsActive)
                return new AuthResponseDTO();

            // generate new Refresh Token
            refreshToken.RevokedOn = DateTime.UtcNow;

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await userManager.UpdateAsync(user);

            // create new JwtToken
            var newJwtTokenResponse = await CreateTokenAsync(user.Id);
            newJwtTokenResponse.RefreshToken = newRefreshToken.Token;
            newJwtTokenResponse.RefreshTokenExpiration = newRefreshToken.ExpiresOn;

            return newJwtTokenResponse;
        }

    }
}
