using HealthCore.DataAccess.IConfiguration;
using HealthCore.Models;
using HealthCore.Models.Incoming;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAll()
        {
            var users = await unitOfWork.Users.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var user = await unitOfWork.Users.GetAsync(u=> u.Id == id);
            if(user == null)
                return NotFound();
            return Ok(user);    
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(AddUserDTO userDto)
        {
            var user = new User
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Phone = userDto.Phone,
                Country = userDto.Country,
                Satus = 1,
                UpdateDate = DateTime.UtcNow,
                BirthDate = userDto.BirthDate
            };

            await unitOfWork.Users.AddAsync(user);
            await unitOfWork.CompleteAsync();
            return CreatedAtAction(nameof(GetAsync), new {id = user.Id}, userDto);
        }
    }
}
