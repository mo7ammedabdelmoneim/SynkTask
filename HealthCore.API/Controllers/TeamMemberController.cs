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
                Id = teamMember.Id,
                FirstName = teamMember.FirstName,
                LastName = teamMember.LastName,
                Country = teamMember.Country,
                Email = teamMember.Email,
                Role = teamMember.Role,
                TeamLeadId = teamMember.TeamLeadId,
            };

            response.Success = true;
            response.Message = "TeamMember Info Retrieved Successfully";
            response.Data = teamMemberResponse;

            return Ok(response);
        }


        [HttpPost("AssignToTeamLead")]
        public async Task<IActionResult> AssignTeamMemberToTeamLead(AssignMemberDto memberDto)
        {
            var response = new ApiResponse<string>();

            if (!ModelState.IsValid)
            {
                response.Message = "Invalid input data";
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(response);
            }

            var teamLead = await unitOfWork.TeamLeads.GetAsync(t => t.Id == memberDto.TeamLeadId);
            var member = await unitOfWork.TeamMembers.GetAsync(m => m.Id == memberDto.TeamMemberId);

            if(teamLead == null || member == null)
            {
                response.Message = "Invalid input data";
                response.Errors = new List<string> { "TeamLeadId or TeamMemberId is Wrong" };
                return BadRequest(response);
            }

            member.TeamLeadId = teamLead.Id;
            unitOfWork.TeamMembers.Update(member);
            await unitOfWork.CompleteAsync();

            response.Success = true;
            response.Message = "TeamMember Assigned successfully";
            return Ok(response);
        }

    }
}
