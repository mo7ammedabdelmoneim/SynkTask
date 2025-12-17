using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SynkTask.DataAccess.IConfiguration;
using SynkTask.Models;
using SynkTask.Models.DTOs;

namespace SynkTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public NotificationController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpPost]
        [ProducesResponseType<ApiResponse<Notification>>(200)]
        public async Task<IActionResult> AddNotificationAsync(AddNotificationDto notificationDto)
        {
            var response = new ApiResponse<Notification>();
            if (!ModelState.IsValid)
            {
                response.Message = "Invalid input data";
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(response);
            }
            var notification = new Notification
            {
                Title = notificationDto.Title,
                UserId = notificationDto.UserId,
                Description = notificationDto.Description,
                Role = notificationDto.Role,
            };
            await unitOfWork.Notifications.AddAsync(notification);
            await unitOfWork.CompleteAsync();

            response.Success = true;
            response.Data = notification;
            response.Message = "Notification Added Successfully";

            return Ok(response);

        }

        
        [HttpGet("{userId:guid}/{role:alpha}")]
        [ProducesResponseType<ApiResponse<List<Notification>>>(200)]
        public async Task<IActionResult> GetUserNotificationsAsync(Guid userId, string role)
        {
            var response = new ApiResponse<List<Notification>>() { Message = "Invalid input data" };
            if (userId == Guid.Empty || role == null)
            {
                response.Errors = new List<string> { "UserId and Role is a must" };

                return BadRequest(response);
            }

            if (role.ToUpper() == "TEAMLEAD")
            {
                var user = await unitOfWork.TeamLeads.GetAsync(t => t.Id == userId);
            }
            else if (role.ToUpper() == "TEAMMEMBER")
            {
                var user = await unitOfWork.TeamMembers.GetAsync(t => t.Id == userId);
            }
            else
            {
                response.Errors = new List<string> { "Invalid Role, Please Enter[TeamLead] or [TeamMember] " };
                return BadRequest(response);
            }

            if(User == null)
            {
                response.Errors = new List<string> { "UserId is wrong" };
                return BadRequest(response);
            }

            var notifications= await unitOfWork.Notifications.GetAllAsync(n=> n.UserId == userId);

            response.Success = true;
            response.Data = notifications.ToList();
            response.Message = "User Notification Retreived Successfully";

            return Ok(response);

        }


    }
}
