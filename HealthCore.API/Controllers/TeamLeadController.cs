using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SynkTask.DataAccess.IConfiguration;
using SynkTask.Models;
using SynkTask.Models.DTOs.Auth;
using SynkTask.Models.DTOs;
using System.ComponentModel;
using System.Threading.Tasks;
using OfficeOpenXml;

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
        
        
        [HttpGet("ExportTeamLeadTasks/{teamLeadId:guid}")]
        public async Task<IActionResult> ExportTeamLeadTasksAsync(Guid teamLeadId)
        {
            var response = new ApiResponse<List<string>>();

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
                DueDate = task.ToDate.ToLocalTime(),
                AssignedMembers = task.AssignedMembers.Select(m => $"{m.FirstName} {m.LastName}({m.Email}), ").ToList(),
            }).OrderBy(t => t.DueDate).ToList();


            // Generate Report
            var fileName = $"tasks_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("tasksResponse");

            // Headers
            worksheet.Cells[1, 1].Value = "Id";
            worksheet.Cells[1, 2].Value = "Title";
            worksheet.Cells[1, 3].Value = "Description";
            worksheet.Cells[1, 4].Value = "Priority";
            worksheet.Cells[1, 5].Value = "Status";
            worksheet.Cells[1, 6].Value = "DueDate";
            worksheet.Cells[1, 7].Value = "AssignedMembers";

            // Data
            for (int i = 0; i < tasksResponse.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = tasksResponse[i].Id;
                worksheet.Cells[i + 2, 2].Value = tasksResponse[i].Title;
                worksheet.Cells[i + 2, 3].Value = tasksResponse[i].Description;
                worksheet.Cells[i + 2, 4].Value = tasksResponse[i].Priority;
                worksheet.Cells[i + 2, 5].Value = tasksResponse[i].Status;
                worksheet.Cells[i + 2, 6].Value = tasksResponse[i].DueDate.ToString("dd/MM/yyyy");
                worksheet.Cells[i + 2, 7].Value = tasksResponse[i].AssignedMembers;
            }

            using var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );

        }
        
        [HttpGet("ExportTeamMembersTasks/{teamLeadId:guid}")]
        public async Task<IActionResult> ExportTeamMembersTasksAsync(Guid teamLeadId)
        {
            var response = new ApiResponse<string>();

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


            // Generate Report
            var fileName = $"teamMembers_tasks_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("membersResponse");

            // Headers
            worksheet.Cells[1, 1].Value = "Id";
            worksheet.Cells[1, 2].Value = "FirstName";
            worksheet.Cells[1, 3].Value = "LastName";
            worksheet.Cells[1, 4].Value = "Email";
            worksheet.Cells[1, 5].Value = "PendingTasks";
            worksheet.Cells[1, 6].Value = "InProgressTasks";
            worksheet.Cells[1, 7].Value = "ComplttedTasks";

            // Data
            for (int i = 0; i < membersResponse.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = membersResponse[i].Id;
                worksheet.Cells[i + 2, 2].Value = membersResponse[i].FirstName;
                worksheet.Cells[i + 2, 3].Value = membersResponse[i].LastName;
                worksheet.Cells[i + 2, 4].Value = membersResponse[i].Email;
                worksheet.Cells[i + 2, 5].Value = membersResponse[i].PendingTasks;
                worksheet.Cells[i + 2, 6].Value = membersResponse[i].InProgressTasks;
                worksheet.Cells[i + 2, 7].Value = membersResponse[i].ComplttedTasks;
            }

            using var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );

        }


    }
}
