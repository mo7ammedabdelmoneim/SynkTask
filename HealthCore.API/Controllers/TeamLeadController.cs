using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SynkTask.DataAccess.IConfiguration;
using SynkTask.Models;
using SynkTask.Models.DTOs.Auth;
using SynkTask.Models.DTOs;

namespace SynkTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamLeadController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public TeamLeadController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("Info/{teamLeadId:guid}")]
        [ProducesResponseType<ApiResponse<GetTeamLeadInfoResponseDto>>(200)]
        public async Task<IActionResult> GetteamLeadInfo(Guid teamLeadId)
        {
            var response = new ApiResponse<GetTeamLeadInfoResponseDto>();

            var teamLead = await unitOfWork.TeamLeads.GetAsync(l => l.Id == teamLeadId);
            if(teamLead == null)
            {
                response.Message = "Invalid Input";
                response.Errors = new List<string>() { "TeamLeadId is Wrong" };
                return BadRequest(response);
            }

            var teamLeadResponse = new GetTeamLeadInfoResponseDto
            {
                Id = teamLeadId,
                ApplicationUserId = teamLead.ApplicationUserId,
                FirstName = teamLead.FirstName,
                LastName = teamLead.LastName,
                Country = teamLead.Country,
                Email = teamLead.Email,
                ImageUrl = teamLead.ImageUrl
            };

            response.Success = true;
            response.Message = "TeamLead Info Retrieved Successfully";
            response.Data = teamLeadResponse;

            return Ok(response);
        }


        [HttpGet("Dashboard/{teamLeadId:guid}")]
        [ProducesResponseType<ApiResponse<GetUserDashboardDataResponseDto>>(200)]
        public async Task<IActionResult> GetTeamLeadDashboardData(Guid teamLeadId)
        {
            var response = new ApiResponse<GetUserDashboardDataResponseDto>();

            var teamLead = await unitOfWork.TeamLeads.GetAsync(l => l.Id == teamLeadId);
            if (teamLead == null)
            {
                response.Message = "Invalid Input";
                response.Errors = new List<string>() { "TeamLeadId is Wrong" };
                return BadRequest(response);
            }

            var tasks = await unitOfWork.ProjectTasks.GetAllAsync(t => t.TeamLeadId == teamLeadId);
            var recentTasks = tasks.OrderByDescending(t => t.FromDate).Select(t => new RecentTasksDto
            {
                Title = t.Title,
                CreatedAt = t.FromDate,
                Status = t.Status,
                Priority = t.Priority
            }).ToList();

            int totalTasks = tasks.Count();
            int pendingTasks = tasks.Count(t => t.Status?.ToLower() == "pending");
            int inProgressTasks = tasks.Count(t => t.Status?.ToLower() == "in progress");
            int completedTasks = tasks.Count(t => t.Status?.ToLower() == "completed");

            // Task Priority
            int lowTasks = tasks.Count(t => t.Priority?.ToLower() == "low");
            int mediumTasks = tasks.Count(t => t.Priority?.ToLower() == "meduim");
            int highTasks = tasks.Count(t => t.Priority?.ToLower() == "high");

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
            response.Message = "TeamLead Dashboard Data Retreived Successfully";
            response.Data = data;

            return Ok(response);
        }


        [HttpGet("Projects/{teamLeadId:guid}")]
        [ProducesResponseType<ApiResponse<List<GetTeamLeadProjectInfoResponseDto>>>(200)]
        public async Task<IActionResult> GetTeamLeadProjectsAsync(Guid teamLeadId)
        {
            var response = new ApiResponse<List<GetTeamLeadProjectInfoResponseDto>>();
            

            var projects = await unitOfWork.Projects.GetAllAsync(p => p.TeamLeadId == teamLeadId,includedProperties: "Tasks");
            if (!projects.Any())
            {
                response.Success = true;
                response.Message = "No Data";
                response.Errors = new List<string>() { "No Projects For This TeamLead" };
                return NotFound(response);
            }

            List<GetTeamLeadProjectInfoResponseDto> projectResponse = projects.Select(prject => new GetTeamLeadProjectInfoResponseDto
            {
                Id = prject.Id,
                Name = prject.Name,
                TeamLeadId = prject.TeamLeadId,
                Description = prject.Description,
                IsActive = prject.IsActive,
                TaskCount = prject.Tasks.Count()
            }).ToList();


            response.Success = true;
            response.Message = "TeamLead Projects Retrieved Successfully";
            response.Data = projectResponse;

            return Ok(response);
        }
        
        
        [HttpGet("Tasks/{teamLeadId:guid}")]
        [ProducesResponseType<ApiResponse<List<GetUserTaskInfoResponseDto>>>(200)]
        public async Task<IActionResult> GetTeamLeadTasksAsync(Guid teamLeadId)
        {
            var response = new ApiResponse<List<GetUserTaskInfoResponseDto>>();

            var tasks = await unitOfWork.ProjectTasks.GetAllAsync(p => p.TeamLeadId == teamLeadId, includedProperties: "AssignedMembers,Todos");
            if (!tasks.Any())
            {
                response.Success = true;
                response.Message = "No Data";
                response.Errors = new List<string>() { "No Tasks For This TeamLead" };
                return NotFound(response);
            }

            List<GetUserTaskInfoResponseDto> tasksResponse = tasks.Select(task => new GetUserTaskInfoResponseDto
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
                CompletedTodos = task.Todos.Count(t=> t.IsCompleted)
            }).OrderBy(t=>t.DueDate).ToList();

            response.Success = true;
            response.Message = "TeamLead Tasks Retrieved Successfully";
            response.Data = tasksResponse;

            return Ok(response);
        }


        [HttpGet("TeamMembers/{teamLeadId:guid}")]
        [ProducesResponseType<ApiResponse<List<GetTeamMemberOfTeamLeadResponseDto>>>(200)]
        public async Task<IActionResult> GetTeamMemerForTeamLeadAsync(Guid teamLeadId)
        {
            var response = new ApiResponse<List<GetTeamMemberOfTeamLeadResponseDto>>();

            var team = await unitOfWork.Teams.GetAsync(t => t.TeamLeadId == teamLeadId);
            if (team == null)
            {
                response.Success = true;
                response.Message = "No Data";
                response.Errors = new List<string>() { "TeamLead does not have teams" };
                return NotFound(response);
            }
            var members = await unitOfWork.TeamMembers.GetAllAsync(p => p.TeamId == team.Id,includedProperties: "ProjectTasks");
            if (!members.Any())
            {
                response.Success = true;
                response.Message = "No Data";
                response.Errors = new List<string>() { "No TeamMembers For This TeamLead" };
                return NotFound(response);
            }

            List<GetTeamMemberOfTeamLeadResponseDto> membersResponse = members.Select(member => new GetTeamMemberOfTeamLeadResponseDto
            {
                Id = member.Id,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Email = member.Email,
                ImageUrl = member.ImageUrl,
                PendingTasks = member.ProjectTasks.Count(t=> t.Status?.ToLower() == "pending"),
                InProgressTasks = member.ProjectTasks.Count(t=> t.Status?.ToLower() == "in progress"),
                ComplttedTasks = member.ProjectTasks.Count(t=> t.Status?.ToLower() == "completed"),
            }).ToList();

            response.Success = true;
            response.Message = "TeamMembers Retrieved Successfully";
            response.Data = membersResponse;

            return Ok(response);
        }
    }
}
