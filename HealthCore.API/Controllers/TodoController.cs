using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SynkTask.DataAccess.IConfiguration;
using SynkTask.Models;
using SynkTask.Models.DTOs;

namespace SynkTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public TodoController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpPost]
        [ProducesResponseType<ApiResponse<GetTodoInfoResponseDto>>(200)]
        public async Task<IActionResult> AddTodoAsync(CreateTodoDto todoDto)
        {
            var response = new ApiResponse<GetTodoInfoResponseDto>();

            if (!ModelState.IsValid)
            {
                response.Message = "Invalid input data";
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(response);
            }

            var task = await unitOfWork.ProjectTasks.GetAsync(t => t.Id == todoDto.TaskId);
            var member = await unitOfWork.TeamMembers.GetAsync(m=> m.Id == todoDto.TeamMemberId);

            if (task == null || member == null)
            {
                response.Message = "Invalid input data";
                response.Errors = new List<string> { "TaskId or TeamMemberId is Wrong." };
                return BadRequest(response);
            }

            var newTodo = new Todo
            {
                Title = todoDto.Title,
                Description = todoDto.Description,
                TaskId = todoDto.TaskId,
                TeamMemberId = todoDto.TeamMemberId,
            };

            await unitOfWork.Todos.AddAsync(newTodo);
            await unitOfWork.CompleteAsync();

            var todoResponse = new GetTodoInfoResponseDto
            {
                Id = newTodo.Id,
                Title = newTodo.Title,
                Description = newTodo.Description,
                TaskId = newTodo.TaskId,
                TeamMemberId = newTodo.TeamMemberId,
                Created = newTodo.Created,
                IsCompleted = newTodo.IsCompleted,
            };

            response.Success = true;
            response.Message = "Todo Added Successfully";
            response.Data = todoResponse;
            
            return Ok(response);
        }


        [HttpGet("Info/{todoId:guid}")]
        [ProducesResponseType<ApiResponse<GetTodoInfoResponseDto>>(200)]
        public async Task<IActionResult> GetTodoInfo(Guid todoId)
        {
            var response = new ApiResponse<GetTodoInfoResponseDto>() { Success = true };
            
            var todo = await unitOfWork.Todos.GetAsync(t=>t.Id ==  todoId);
            if(todo == null)
            {
                response.Message = "No data";
                response.Errors = new List<string> { "No Todo for this Id." };
                return NotFound(response);
            }

            var todoResponse = new GetTodoInfoResponseDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                TaskId = todo.TaskId,
                TeamMemberId = todo.TeamMemberId,
                Created = todo.Created,
                IsCompleted = todo.IsCompleted,
            };

            response.Success = true;
            response.Message = "Todo Info Retrieved Successfully";
            response.Data = todoResponse;

            return Ok(response);
        }

       
    }
}
