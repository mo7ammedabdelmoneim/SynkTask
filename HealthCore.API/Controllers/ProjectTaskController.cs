using AutoMapper.Execution;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SynkTask.API.Configurations;
using SynkTask.API.Services.IService;
using SynkTask.DataAccess.IConfiguration;
using SynkTask.DataAccess.Repository.IRepository;
using SynkTask.Models.DTOs;
using SynkTask.Models.Models;
using System.Net.Mail;

namespace SynkTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectTaskController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IEmailService emailRepository;
        private readonly IFileStorageService fileStorageService;

        public ProjectTaskController(IUnitOfWork unitOfWork, IEmailService emailRepository, IFileStorageService fileStorageService)
        {
            this.unitOfWork = unitOfWork;
            this.emailRepository = emailRepository;
            this.fileStorageService = fileStorageService;
        }

        [HttpGet("Info/{taskId:guid}")]
        [ProducesResponseType<ApiResponse<GetTaskInfoResponseDto>>(200)]
        public async Task<IActionResult> GetTaskInfo(Guid taskId)
        {
            var response = new ApiResponse<GetTaskInfoResponseDto>();

            var task = await unitOfWork.ProjectTasks.GetAsync(t => t.Id == taskId, includedProperties: "AssignedMembers,Todos,Attachments");
            if (task == null)
            {
                response.Message = "Invalid Input";
                response.Errors = new List<string>() { "TaskId is Wrong" };
                return BadRequest(response);
            }

            var taskResponse = new GetTaskInfoResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.ToDate,
                FromDate = task.FromDate,
                Priority = task.Priority,
                Status = task.Status,
                ProjectId = task.ProjectId,
                TeamLeadId = task.TeamLeadId,
                AssignedMemebersPicture = task.AssignedMembers.Select(m => m.ImageUrl).ToList(),
                Todos = task.Todos.Select(t => new GetTodoInfoResponseDto
                {
                    Id = t.Id,
                    Description = t.Description,
                    Title = t.Title,
                    Created = t.Created,
                    IsCompleted = t.IsCompleted,
                    TaskId = t.TaskId,
                    TeamMemberId = t.TeamMemberId
                }).ToList(),
                Attachments = task.Attachments.Select(a => new TaskAttachmentDto
                {
                    Id = a.Id,
                    FileUrl = a.FileUrl,
                    FileName = a.FileName,
                    FileType = a.ResourceType
                }).ToList(),
            };

            response.Success = true;
            response.Message = "Task Info Retrieved Successfully";
            response.Data = taskResponse;

            return Ok(response);
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
            var taskMembers = new List<TeamMember>();
            var team = await unitOfWork.Teams.GetAsync(t => t.TeamLeadId == project.TeamLeadId);
            var allTeamMembers = await unitOfWork.TeamMembers.GetAllAsync(m => m.TeamId == team.Id);
            foreach (var email in taskDto.AssignedMembersEmail)
            {
                var member = allTeamMembers.FirstOrDefault(m => m.Email == email);
                if (member != null)
                    taskMembers.Add(member);
            }

            if (project == null || !taskMembers.Any())
            {
                response.Message = "Invalid input data";
                response.Errors = new List<string> { "ProjectId or AssignedMembers is Wrong." };
                return BadRequest(response);
            }

            var newProjectTask = new ProjectTask
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                TeamLeadId = project.TeamLeadId,
                ProjectId = taskDto.ProjectId,
                FromDate = taskDto.FromDate,
                ToDate = taskDto.DueDate,
                Priority = taskDto.Priority ?? "Esay",
                Status = taskDto.Status = ProjectTaskStatus.Pending,
                AssignedMembers = taskMembers
            };
            await unitOfWork.ProjectTasks.AddAsync(newProjectTask);

            // send notifications and emails 
            var projectEntity = await unitOfWork.Projects.GetAsync(p => p.Id == project.Id);
            var projectName = projectEntity.Name;
            var teamLead = await unitOfWork.TeamLeads.GetAsync(p => p.Id == team.TeamLeadId);
            var teamLeadName = teamLead.FirstName;
            foreach (var member in taskMembers)
            {
                var notification = new Notification
                {
                    CreatedAt = DateTime.UtcNow,
                    Title = "New Task Assigned",
                    Description = $"You have been assigned a new task \"{newProjectTask.Title}\" in project \"{projectName}\".\r\nPlease review and start working on it.",
                    UserId = member.Id,
                    Role = Roles.TeamMember
                };
                await unitOfWork.Notifications.AddAsync(notification);

                await emailRepository.SendNewTaskEmailEmailAsync(member.Email, member.FirstName, projectName, newProjectTask.Title, teamLeadName, newProjectTask.ToDate.ToString("dd/MM/yyyy"));
            }


            await unitOfWork.CompleteAsync();

            var responseData = new GetProjectTaskInfoResponseDto
            {
                Id = newProjectTask.Id,
                Title = newProjectTask.Title,
                Description = newProjectTask.Description,
                TeamLeadId = newProjectTask.TeamLeadId,
                ProjectId = newProjectTask.ProjectId,
                FromDate = newProjectTask.FromDate,
                DueDate = newProjectTask.ToDate,
                Priority = newProjectTask.Priority,
                Status = newProjectTask.Status,
                AssignedMemebers = taskMembers.Select(m => new GetTaskMemberDto
                {
                    MemberId = m.Id,
                    MemberName = m.FirstName,
                    Email = m.Email,
                    ImageUrl = m.ImageUrl
                }).ToList()
            };

            response.Message = "Project Task Added Successfully";
            response.Success = true;
            response.Data = responseData;

            return Ok(response);
        }


        [HttpPost("Attachments/{taskId:guid}")]
        [ProducesResponseType<ApiResponse<List<TaskAttachmentDto>>>(200)]
        public async Task<IActionResult> UploadAttachments(Guid taskId, [FromForm] List<IFormFile> files)
        {
            var response = new ApiResponse<List<TaskAttachmentDto>>();
            if (!files.Any())
            {
                response.Message = "No files uploaded";
                response.Errors = new List<string> { "No data to uploade" };
                return BadRequest(response);
            }

            var attachments = new List<TaskAttachment>();

            foreach (var file in files)
            {
                var upload = await fileStorageService.UploadAsync(file, taskId);

                var attachment = new TaskAttachment
                {
                    TaskId = taskId,
                    FileName = file.FileName,
                    FileUrl = upload.Url,
                    PublicId = upload.PublicId,
                    ResourceType = upload.ResourceType,
                    FileSize = file.Length,
                    ContentType = file.ContentType
                };
                attachments.Add(attachment);
                await unitOfWork.TaskAttachments.AddAsync(attachment);
            }
            await unitOfWork.CompleteAsync();

            var responseData = attachments.Select(a => new TaskAttachmentDto
            {
                Id = a.Id,
                FileUrl = a.FileUrl,
                FileName = a.FileName,
                FileType = a.ResourceType
            }).ToList();

            response.Success = true;
            response.Message = "Data uploaded successfull";
            response.Data = responseData;

            return Ok(response);
        }


        [HttpPut]
        [ProducesResponseType<ApiResponse<GetProjectTaskInfoResponseDto>>(200)]
        public async Task<IActionResult> UpdateProjectTask(UpdateProjectTaskDto taskDto)
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

            var task = await unitOfWork.ProjectTasks.GetAsync(t => t.Id == taskDto.TaskId);
            if (task == null)
            {
                response.Message = "Invalid input data";
                response.Errors = new List<string> { "TaskId is Wrong." };
                return BadRequest(response);
            }

            var taskMembers = new List<TeamMember>();
            var team = await unitOfWork.Teams.GetAsync(t => t.TeamLeadId == task.TeamLeadId);
            var allTeamMembers = await unitOfWork.TeamMembers.GetAllAsync(m => m.TeamId == team.Id);
            foreach (var email in taskDto.AssignedMembersEmail)
            {
                var member = allTeamMembers.FirstOrDefault(m => m.Email == email);
                if (member != null)
                    taskMembers.Add(member);
            }

            if (!taskMembers.Any())
            {
                response.Message = "Invalid input data";
                response.Errors = new List<string> { "AssignedMembers are not found." };
                return BadRequest(response);
            }

            await unitOfWork.ProjectTasks.ResetAssignedMembersAsync(task.Id);

            task.Title = taskDto.Title;
            task.Description = taskDto.Description;
            task.Priority = taskDto.Priority;
            task.ToDate = taskDto.DueDate;
            task.AssignedMembers = taskMembers;

            unitOfWork.ProjectTasks.Update(task);
            await unitOfWork.CompleteAsync();

            var responseData = new GetProjectTaskInfoResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                TeamLeadId = task.TeamLeadId,
                ProjectId = task.ProjectId,
                FromDate = task.FromDate,
                DueDate = task.ToDate,
                Priority = task.Priority,
                Status = task.Status,
                AssignedMemebers = taskMembers.Select(m => new GetTaskMemberDto
                {
                    MemberId = m.Id,
                    MemberName = m.FirstName,
                    Email = m.Email,
                    ImageUrl = m.ImageUrl
                }).ToList()
            };

            response.Message = "Project Task Updated Successfully";
            response.Success = true;
            response.Data = responseData;

            return Ok(response);
        }


        [HttpDelete("{taskId:guid}")]
        public async Task<IActionResult> DeleteProjectTask(Guid taskId)
        {
            var task = await unitOfWork.ProjectTasks.GetAsync(t => t.Id == taskId);
            if (task == null)
            {
                return BadRequest("TaskId is wrong.");
            }

            await unitOfWork.ProjectTasks.Delete(task);
            await unitOfWork.CompleteAsync();

            return NoContent();
        }

        [HttpPut("UpdateStatus")]
        public async Task<IActionResult> UpdateTaskStatus(UpdateTaskStatusDto taskDto)
        {
            var response = new ApiResponse<string>();
            var task = await unitOfWork.ProjectTasks.GetAsync(t => t.Id == taskDto.TaskId, includedProperties: "AssignedMembers");
            if (task == null)
            {
                response.Message = "Invalid Input";
                response.Errors = new List<string>() { "TaskId is Wrong" };
                return BadRequest(response);
            }

            task.Status = taskDto.Status;

            // Send Notification and Emails to teamMembers
            var projectEntity = await unitOfWork.Projects.GetAsync(p => p.Id == task.ProjectId);
            var projectName = projectEntity.Name;
            foreach (var member in task.AssignedMembers)
            {
                var notification = new Notification
                {
                    CreatedAt = DateTime.Now,
                    Title = " Task Status Updated",
                    Description = $"The status of task \"{task.Title}\" has been updated to \"{task.Status}\".",
                    UserId = member.Id,
                    Role = Roles.TeamMember
                };
                await unitOfWork.Notifications.AddAsync(notification);

                await emailRepository.SendTaskUpdatedEmailEmailAsync(member.Email, member.FirstName, projectName, task.Title, task.Status);
            }

            // Send Notification and Emails to TeamLead if Task is Completed
            if (task.Status.ToLower() == ProjectTaskStatus.Completed.ToLower())
            {
                var notification = new Notification
                {
                    CreatedAt = DateTime.Now,
                    Title = "Task Completed",
                    Description = $"Task \"{task.Title}\" in project \"{projectName}\" has been completed.",
                    UserId = task.TeamLeadId,
                    Role = Roles.TeamLead
                };
                await unitOfWork.Notifications.AddAsync(notification);
                var teamlead = await unitOfWork.TeamLeads.GetAsync(l => l.Id == task.TeamLeadId);
                await emailRepository.SendTaskCompletedEmailEmailAsync(teamlead.Email, teamlead.FirstName, projectName, task.Title);
            }

            await unitOfWork.CompleteAsync();

            response.Success = true;
            response.Message = "Task Status Updated Sucssfully";
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
