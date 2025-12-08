using HealthCore.API.Configurations.Models;
using HealthCore.DataAccess.IConfiguration;
using HealthCore.Models;
using HealthCore.Models.DTOs.Outcoming;
using HealthCore.Models.Incoming;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;

namespace HealthCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly JwtConfig jwtConfig;

        public AccountController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, IOptionsMonitor<JwtConfig> optionsMonitor, IConfiguration configuration )
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.configuration = configuration;
            this.jwtConfig = optionsMonitor.CurrentValue;
        }

        [HttpPost("Register")]
        [ProducesResponseType(typeof(UserRegisterResponse)  , 200 )]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            var result = new UserRegisterResponse { Success = false };
            if (!ModelState.IsValid)
            {
                result.Errors = ModelState.Select(m => m.Value.ToString()).ToList();
                return BadRequest(result);
            }

            var user = await userManager.FindByEmailAsync(registerDTO.Email);
            if(user != null)
            {
                result.Errors = new List<string>() {"User is Already Exist!"};
                return BadRequest(result);
            }
            user = new IdentityUser
            {
                UserName = registerDTO.Email,
                Email = registerDTO.Email,
                EmailConfirmed = true
            };
            var addResult = await userManager.CreateAsync(user, registerDTO.Password);
            if (!addResult.Succeeded)
            {
                result.Errors = addResult.Errors.Select(e => e.Description).ToList();
                return BadRequest(result);
            }

            // Add user to Users Table in DB
            var appUser = new User
            {
                FirstName = registerDTO.FirstName,
                LastName = registerDTO.LastName,
                Email = registerDTO.Email,
                BirthDate = DateTime.UtcNow,
                IdentityId = user.Id,
                Satus = 1,
                Country = "",
                Phone = "",
            };
            await unitOfWork.Users.AddAsync(appUser);
            await unitOfWork.CompleteAsync();

            var token = CreateToken(user);

            result.Success = true;
            result.Token = token;
            return Ok(result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            var result = new UserLoginResponse { Success = false };

            if (!ModelState.IsValid)
            {
                result.Errors = ModelState.Select(m => m.Value.ToString()).ToList();
                return BadRequest(result);
            }

            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if(user == null)
            {
                result.Errors = new List<string>() { "Email Or Password is Wrong." };
                return BadRequest(result);
            }

            var check = await userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!check)
            {
                result.Errors = new List<string>() { "Email Or Password is Wrong." };
                return BadRequest(result);
            }

            result.Success = true;
            result.Token = CreateToken(user);
            return Ok(result);
        }




        string CreateToken(IdentityUser user)
        {
            var key = Encoding.ASCII.GetBytes(jwtConfig.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier,user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub,user.Email),
                    new Claim(JwtRegisteredClaimNames.Email,user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256
                )
            };

            // generate the security obj token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            // convert token obj to string
            return tokenHandler.WriteToken(token);
        }
    }
}
