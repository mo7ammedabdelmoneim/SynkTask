using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SynkTask.DataAccess.Data;
using SynkTask.DataAccess.IConfiguration;
using SynkTask.Models.DTOs;
using SynkTask.Models;

namespace SynkTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamMemberController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public TeamMemberController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("Info/{teamMemberId:guid}")]
        [ProducesResponseType<ApiResponse<GetTeamMemberInfoResponseDto>>(200)]
        public async Task<IActionResult> GetTeamMemberInfo(Guid teamMemberId)
        {
            var response = new ApiResponse<GetTeamMemberInfoResponseDto>();

            var teamMember = await unitOfWork.TeamMembers.GetAsync(l => l.Id == teamMemberId);
            if (teamMember == null)
            {
                response.Message = "Invalid Input";
                response.Errors = new List<string>() { "TeamMemberId is Wrong" };
                return BadRequest(response);
            }

            var teamMemberResponse = new GetTeamMemberInfoResponseDto
            {
                Id = teamMemberId,
                FirstName = teamMember.FirstName,
                LastName = teamMember.LastName,
                Country = teamMember.Country,
                Email = teamMember.Email,
                ImageUrl = teamMember.ImageUrl
            };

            response.Success = true;
            response.Message = "TeamMember Info Retrieved Successfully";
            response.Data = teamMemberResponse;

            return Ok(response);
        }


        [HttpGet("Dashboard/{teamMemberId:guid}")]
        [ProducesResponseType<ApiResponse<GetUserDashboardDataResponseDto>>(200)]
        public async Task<IActionResult> GetTeamMemberDashboardData(Guid teamMemberId)
        {
            var response = new ApiResponse<GetUserDashboardDataResponseDto>();

            var teamMember = await unitOfWork.TeamMembers.GetAsync(l => l.Id == teamMemberId, includedProperties: "ProjectTasks");
            if (teamMember == null)
            {
                response.Message = "Invalid Input";
                response.Errors = new List<string>() { "TeamMemberId is Wrong" };
                return BadRequest(response);
            }

            var recentTasks = teamMember.ProjectTasks.OrderByDescending(t => t.FromDate).Select(t => new RecentTasksDto
            {
                Title = t.Title,
                CreatedAt = t.FromDate,
                Status = t.Status,
                Priority = t.Priority
            }).ToList();

            int totalTasks = teamMember.ProjectTasks.Count();
            int pendingTasks = teamMember.ProjectTasks.Count(t => t.Status?.ToLower() == "pending");
            int inProgressTasks = teamMember.ProjectTasks.Count(t => t.Status?.ToLower() == "in progress");
            int completedTasks = teamMember.ProjectTasks.Count(t => t.Status?.ToLower() == "completed");

            // Task Priority
            int lowTasks = teamMember.ProjectTasks.Count(t => t.Priority?.ToLower() == "low");
            int mediumTasks = teamMember.ProjectTasks.Count(t => t.Priority?.ToLower() == "meduim");
            int highTasks = teamMember.ProjectTasks.Count(t => t.Priority?.ToLower() == "high");

            var data = new GetUserDashboardDataResponseDto
            {
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                PendingTasks = pendingTasks,
                InProgressTasks = inProgressTasks,
                HighTasks = highTasks,
                LowTasks = lowTasks,
                MediumTasks = mediumTasks,
                RecentTasks = recentTasks,
            };

            response.Success = true;
            response.Message = "TeamMember Dashboard Data Retreived Successfully";
            response.Data = data;

            return Ok(response);
        }


        [HttpGet("Tasks/{teamMemberId:guid}")]
        [ProducesResponseType<ApiResponse<List<GetTeamMemberOfTeamLeadResponseDto>>>(200)]
        public async Task<IActionResult> GetTeamMemberTasksAsync(Guid teamMemberId)
        {
            var response = new ApiResponse<List<GetUserTaskInfoResponseDto>>();

            var teamMember = await unitOfWork.TeamMembers.GetTeamMemberWithTasksAsync(teamMemberId);
            if (teamMember == null)
            {
                response.Success = false;
                response.Message = "Invalid Input Data";
                response.Errors = new List<string>() { "TeamMemberId is wrong" };
                return BadRequest(response);
            }
            if (!teamMember.ProjectTasks.Any())
            {
                response.Success = true;
                response.Message = "No Data";
                response.Errors = new List<string>() { "No Tasks For This TeamMember" };
                return NotFound(response);
            }

            List<GetUserTaskInfoResponseDto> tasksResponse = teamMember.ProjectTasks.Select(task => new GetUserTaskInfoResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Priority = task.Priority,
                Description = task.Description,
                Status = task.Status,
                FromDate = task.FromDate,
                DueDate = task.ToDate,
                AssignedMembersPicture = task.AssignedMembers.Select(m => m.ImageUrl).ToList(),
                Todos = task.Todos.Count(),
                CompletedTodos = task.Todos.Count(t => t.IsCompleted)
            }).OrderBy(t => t.DueDate).ToList();

            response.Success = true;
            response.Message = "TeamMember Tasks Retrieved Successfully";
            response.Data = tasksResponse;

            return Ok(response);
        }

        


    }
}
