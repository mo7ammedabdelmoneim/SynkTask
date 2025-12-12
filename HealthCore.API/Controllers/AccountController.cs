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
using SynkTask.DataAccess.IRepository;
using Microsoft.AspNetCore.Authorization;

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
                response.Message = "Email already exists";
                return BadRequest(response);
            }

            var newUser = new ApplicationUser
            {
                UserName = registerDTO.UserName,
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

            response.Success = true;
            response.Message = "User registered successfully";
            response.Data = await CreateTokenAsync(newUser.Id);

            return Ok(response);
        }

        [HttpPost("Login")]
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

            response.Success = true;
            response.Message = "User Login successfully";
            response.Data = await CreateTokenAsync(user.Id);
            return Ok(response);
        }

        async Task<AuthResponseDTO> CreateTokenAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            AuthResponseDTO response = new();
            SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecurityKey"]));
            SigningCredentials signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
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
                expires: DateTime.Now.AddHours(1),
                signingCredentials: signingCredentials
            );

            response = new AuthResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                UserName = user.UserName,
                Email = user.Email,
                Expiration = DateTime.Now.AddHours(1),
                Roles = roles
            };

            return response;
        }

    }
}
