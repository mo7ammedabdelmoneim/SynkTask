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
                Role = teamLead.Role,
            };

            response.Success = true;
            response.Message = "TeamLead Info Retrieved Successfully";
            response.Data = teamLeadResponse;

            return Ok(response);
        }

        [HttpGet("TaskMemers/{teamLeadId:guid}")]
        [ProducesResponseType<ApiResponse<List<GetTeamMemberInfoResponseDto>>>(200)]
        public async Task<IActionResult> GetTeamMemerForTeamLeadAsync(Guid teamLeadId)
        {
            var response = new ApiResponse<List<GetTeamMemberInfoResponseDto>>();

            var members = await unitOfWork.TeamMembers.GetAllAsync(p => p.TeamLeadId == teamLeadId);
            if (!members.Any())
            {
                response.Success = true;
                response.Message = "No Data";
                response.Errors = new List<string>() { "No TeamMembers For This TeamLead" };
                return NotFound(response);
            }

            List<GetTeamMemberInfoResponseDto> membersResponse = members.Select(member => new GetTeamMemberInfoResponseDto
            {
                Id = member.Id,
                FirstName = member.FirstName,
                TeamLeadId = member.TeamLeadId,
                Country = member.Country,
                Email = member.Email,
                LastName = member.LastName,
                Role = member.Role,
            }).ToList();


            response.Success = true;
            response.Message = "TeamMembers Retrieved Successfully";
            response.Data = membersResponse;

            return Ok(response);
        }
    }
}
