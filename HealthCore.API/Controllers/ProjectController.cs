using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SynkTask.DataAccess.IConfiguration;
using SynkTask.Models.DTOs;
using SynkTask.Models;

namespace SynkTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public ProjectController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }


        [HttpPost]
        [ProducesResponseType<ApiResponse<GetProjectInfoResponseDto>>(200)]
        public async Task<IActionResult> CreateProject(CreateProjectDto projectDto)
        {
            var response = new ApiResponse<GetProjectInfoResponseDto>();

            if (!ModelState.IsValid)
            {
                response.Message = "Invalid input data";
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(response);
            }

            var teamLead = unitOfWork.TeamLeads.GetAsync(tl=> tl.Id ==  projectDto.TeamLeadId);
            if (teamLead == null)
            {
                response.Message = "Invalid input data";
                response.Errors = new List<string> { "TeamleadId is Wrong." };
                return BadRequest(response);
            }

            var newProject = new Project
            {
                Name = projectDto.ProjectName,
                Description = projectDto.ProjectDescription,
                TeamLeadId = projectDto.TeamLeadId,
            };
            await unitOfWork.Projects.AddAsync(newProject);
            await unitOfWork.CompleteAsync();

            var responseData = new GetProjectInfoResponseDto
            {
                Id = newProject.Id,
                Name = newProject.Name,
                Description = newProject.Description,
                TeamLeadId = newProject.TeamLeadId,
            };

            response.Message = "Project Added Successfully";
            response.Success = true;
            response.Data = responseData;

            return Ok(responseData);
        }


        [HttpGet("Info/{projectId:guid}")]
        [ProducesResponseType<ApiResponse<GetProjectInfoResponseDto>>(200)]
        public async Task<IActionResult> GetProjectInfoAsync(Guid projectId)
        {
            var response = new ApiResponse<GetProjectInfoResponseDto>();
            
            var project = await unitOfWork.Projects.GetAsync(p => p.Id == projectId);
            if (project == null)
            {
                response.Message = "Invalid Input";
                response.Errors = new List<string>() { "projectId is Wrong" };
                return BadRequest(response);
            }

            var projectResponse = new GetProjectInfoResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                TeamLeadId = project.TeamLeadId,
                Description = project.Description,
                IsActive = project.IsActive
            };

            response.Success = true;
            response.Message = "Project Info Retrieved Successfully";
            response.Data = projectResponse;

            return Ok(response);
        }
        
        
        [HttpGet("Tasks/{projectId:guid}")]
        [ProducesResponseType<ApiResponse<List<GetProjectTaskInfoResponseDto>>>(200)]
        public async Task<IActionResult> GetProjectTasksAsync(Guid projectId)
        {
            var response = new ApiResponse<List<GetProjectTaskInfoResponseDto>>();
            
            var Tasks = await unitOfWork.ProjectTasks.GetAllAsync(p => p.ProjectId == projectId);
            if (!Tasks.Any())
            {
                response.Success = true;
                response.Message = "No Data";
                response.Errors = new List<string>() { "No Tasks For This Project" };
                return NotFound(response);
            }

            List<GetProjectTaskInfoResponseDto> projectResponse = Tasks.Select(task => new GetProjectTaskInfoResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                TeamLeadId = task.TeamLeadId,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
               // AssignedMemberId = task.AssignedMemberId,
                FromDate = task.FromDate,
                ToDate = task.ToDate,
                Priority = (Priority)task.Priority,
                ProjectId = projectId,
            }).ToList();
            

            response.Success = true;
            response.Message = "Project Tasks Retrieved Successfully";
            response.Data = projectResponse;

            return Ok(response);
        }


    }
}
