//using SynkTask.DataAccess.IConfiguration;
//using SynkTask.Models;
//using SynkTask.Models.DTOs;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace SynkTask.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class UserController : ControllerBase
//    {
//        private readonly IUnitOfWork unitOfWork;

//        public UserController(IUnitOfWork unitOfWork)
//        {
//            this.unitOfWork = unitOfWork;
//        }

//        [HttpGet]
//        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
//        public async Task<IActionResult> GetAll()
//        {
//            var users = await unitOfWork.Users.GetAllAsync();
//            return Ok(users);
//        }

//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetAsync(string id)
//        {
//            var user = await unitOfWork.Users.GetAsync(u=> u.Id == id);
//            if(user == null)
//                return NotFound();
//            return Ok(user);    
//        }

//        [HttpPost]
//        public async Task<IActionResult> AddAsync(AddUserDTO userDto)
//        {
//            var user = new ApplicationUser
//            {
//                FirstName = userDto.FirstName,
//                LastName = userDto.LastName,
//                Email = userDto.Email,
//                Phone = userDto.Phone,
//                Country = userDto.Country,
//                Satus = 1,
//                UpdateDate = DateTime.UtcNow,
//                BirthDate = userDto.BirthDate
//            };

//            await unitOfWork.Users.AddAsync(user);
//            await unitOfWork.CompleteAsync();
//            return CreatedAtAction(nameof(GetAsync), new {id = user.Id}, userDto);
//        }
//    }
//}
