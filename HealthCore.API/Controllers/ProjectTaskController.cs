using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SynkTask.DataAccess.IConfiguration;
using SynkTask.Models;
using SynkTask.Models.DTOs;

namespace SynkTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectTaskController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public ProjectTaskController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpPost]
        [ProducesResponseType<ApiResponse<GetProjectTaskInfoResponseDto>>(200)]
        public async Task<IActionResult> CreateProjectTask(CreateProjectTaskDto taskDto)
        {
            var response = new ApiResponse<GetProjectTaskInfoResponseDto>();

            if (!ModelState.IsValid)
            {
                response.Message = "Invalid input data";
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(response);
            }

            var project = await unitOfWork.Projects.GetAsync(p => p.Id == taskDto.ProjectId);
            var member = await unitOfWork.TeamMembers.GetAsync(m => m.Id == taskDto.AssignedMemberId);

            if (project == null || member == null)
            {
                response.Message = "Invalid input data";
                response.Errors = new List<string> { "ProjectId or AssignedMemberId is Wrong." };
                return BadRequest(response);
            }

            var newProjectTask = new ProjectTask
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                TeamLeadId = project.TeamLeadId,
                ProjectId = taskDto.ProjectId,
                AssignedMemberId = taskDto.AssignedMemberId,
                FromDate = taskDto.FromDate,
                ToDate = taskDto.ToDate,
                Priority = taskDto.Priority,
                IsCompleted = taskDto.IsCompleted,
            };
            await unitOfWork.ProjectTasks.AddAsync(newProjectTask);
            await unitOfWork.CompleteAsync();

            var responseData = new GetProjectTaskInfoResponseDto
            {
                Id = newProjectTask.Id,
                Title = newProjectTask.Title,
                Description = newProjectTask.Description,
                TeamLeadId = newProjectTask.TeamLeadId,
                ProjectId = newProjectTask.ProjectId,
                AssignedMemberId = newProjectTask.AssignedMemberId,
                FromDate = newProjectTask.FromDate,
                ToDate = newProjectTask.ToDate,
                Priority =(Priority)newProjectTask.Priority,
                IsCompleted = newProjectTask.IsCompleted,
            };

            response.Message = "Project Task Added Successfully";
            response.Success = true;
            response.Data = responseData;

            return Ok(responseData);
        }


        [HttpGet("Info/{projectTaskId:guid}")]
        [ProducesResponseType<ApiResponse<GetProjectTaskInfoResponseDto>>(200)]
        public async Task<IActionResult> GetProjectTaskInfoAsync(Guid projectTaskId)
        {
            var response = new ApiResponse<GetProjectTaskInfoResponseDto>();

            var projectTask = await unitOfWork.ProjectTasks.GetAsync(p => p.Id == projectTaskId);
            if (projectTask == null)
            {
                response.Message = "Invalid Input";
                response.Errors = new List<string>() { "ProjectTaskId is Wrong" };
                return BadRequest(response);
            }

            var projectTaskResponse = new GetProjectTaskInfoResponseDto
            {
                Id = projectTask.Id,
                Title = projectTask.Title,
                Description = projectTask.Description,
                TeamLeadId = projectTask.TeamLeadId,
                ProjectId = projectTask.ProjectId,
                AssignedMemberId = projectTask.AssignedMemberId,
                FromDate = projectTask.FromDate,
                ToDate = projectTask.ToDate,
                Priority = (Priority)projectTask.Priority,
                IsCompleted = projectTask.IsCompleted,
            };

            response.Success = true;
            response.Message = "Project Task Info Retrieved Successfully";
            response.Data = projectTaskResponse;

            return Ok(response);
        }


        [HttpGet("Todos/{projectTaskId:guid}")]
        [ProducesResponseType<ApiResponse<List<GetTodoInfoResponseDto>>>(200)]
        public async Task<IActionResult> GetProjectTaskTodosAsync(Guid projectTaskId)
        {
            var response = new ApiResponse<List<GetTodoInfoResponseDto>>();

            var Todos = await unitOfWork.Todos.GetAllAsync(p => p.TaskId == projectTaskId);
            if (!Todos.Any())
            {
                response.Success = true;
                response.Message = "No Data";
                response.Errors = new List<string>() { "No Todos For This Task" };
                return NotFound(response);
            }

            List<GetTodoInfoResponseDto> taskResponse = Todos.Select(todo => new GetTodoInfoResponseDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                TaskId = todo.TaskId,
                TeamMemberId = todo.TeamMemberId,
                Created = todo.Created,
                IsCompleted = todo.IsCompleted,
            }).ToList();


            response.Success = true;
            response.Message = "Task Todos Retrieved Successfully";
            response.Data = taskResponse;

            return Ok(response);
        }

    }
}
