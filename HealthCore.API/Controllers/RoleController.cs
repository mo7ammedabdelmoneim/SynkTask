using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using SynkTask.DataAccess.IConfiguration;
using SynkTask.Models;
using SynkTask.Models.DTOs;
using SynkTask.Models.DTOs.Auth;
using SynkTask.Models.Models;

namespace SynkTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUnitOfWork unitOfWork;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("All")]
        [ProducesResponseType<ApiResponse<List<string>>>(200)]
        public IActionResult GetRoles()
        {
            var roles = roleManager.Roles.Select(r => r.Name).ToList();

            var response = new ApiResponse<List<string>>()
            {
                Message = "Roles Retrieved Successfully",
                Success = true,
                Data = roles
            };

            return Ok(response);

        }
 
        [HttpGet("UpdateToTeamLead/{TeamMemberEmail}")]
        [ProducesResponseType<ApiResponse<UpdateToTeamleadResponseDto>>(200)]
        public async Task<IActionResult> UpdateToTeamLeadAsync(string TeamMemberEmail)
        {
            var response = new ApiResponse<UpdateToTeamleadResponseDto>();

            var newRole = await roleManager.FindByNameAsync("teamLead");

            var user = await userManager.FindByEmailAsync(TeamMemberEmail);
            if(user == null || newRole == null)
            {
                response.Message = "Invalid input data";
                response.Errors = new List<string> { "Email is Wrong." };
                return BadRequest(response);
            }

            var oldRoles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, oldRoles);
            if (!result.Succeeded)
            {
                response.Message = "Update Role failed";
                response.Errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(response);
            }

            result = await userManager.AddToRoleAsync(user, newRole?.Name);
            if (!result.Succeeded)
            {
                response.Message = "Update Role failed";
                response.Errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(response);
            }

            var teamMember = await unitOfWork.TeamMembers.GetAsync(m => m.Email == TeamMemberEmail);
            if (teamMember == null)
            {
                response.Message = "Update Role failed";
                response.Errors = new List<string> { "No TeamMember with This Email!" };
                return BadRequest(response);
            }

            var newTeamLead = new TeamLead
            {
                FirstName = teamMember.FirstName,
                LastName = teamMember.LastName,
                Country = teamMember.Country,
                Email = teamMember.Email,
                ApplicationUser = teamMember.ApplicationUser,
                ImageUrl = teamMember.ImageUrl,
            };
            await unitOfWork.TeamLeads.AddAsync(newTeamLead);
            await unitOfWork.CompleteAsync();

            var identityApplicationUser = await unitOfWork.IdentityApplicationUsers.GetAsync(u=> u.IdentityUserId == teamMember.ApplicationUserId);
            if (identityApplicationUser == null)
            {
                response.Message = "Update Role failed";
                response.Errors = new List<string> { "Some Thing Went Wrong" };
                return BadRequest(response);
            }

            identityApplicationUser.ApplicationUserId = newTeamLead.Id;
            unitOfWork.IdentityApplicationUsers.Update(identityApplicationUser);

            unitOfWork.TeamMembers.Delete(teamMember);
            await unitOfWork.CompleteAsync();

            var responseData = new UpdateToTeamleadResponseDto
            {
                FirstName = newTeamLead.FirstName,
                LastName = newTeamLead.LastName,
                Email = newTeamLead.Email,
                NewId = newTeamLead.Id, 
                ImageUrl = teamMember.ImageUrl
            };

            response.Success = true;
            response.Message = "Role Updated Successfully";
            response.Data = responseData;
            return Ok(response);
        }
        
        [HttpGet("UpdateToTeamMember/{TeamLeadEmail}")]
        [ProducesResponseType<ApiResponse<UpdateToTeamleadResponseDto>>(200)]
        public async Task<IActionResult> UpdateToTeamMemberAsync(string TeamLeadEmail)
        {
            var response = new ApiResponse<UpdateToTeamleadResponseDto>();

            var newRole = await roleManager.FindByNameAsync("teamMember");

            var user = await userManager.FindByEmailAsync(TeamLeadEmail);
            if(user == null || newRole == null)
            {
                response.Message = "Invalid input data";
                response.Errors = new List<string> { "Email is Wrong." };
                return BadRequest(response);
            }

            var oldRoles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, oldRoles);
            if (!result.Succeeded)
            {
                response.Message = "Update Role failed";
                response.Errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(response);
            }

            result = await userManager.AddToRoleAsync(user, newRole?.Name);
            if (!result.Succeeded)
            {
                response.Message = "Update Role failed";
                response.Errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(response);
            }

            var teamLead = await unitOfWork.TeamLeads.GetAsync(m => m.Email == TeamLeadEmail);
            if (teamLead == null)
            {
                response.Message = "Update Role failed";
                response.Errors = new List<string> { "No TeamLead with This Email!" };
                return BadRequest(response);
            }

            var newTeamMember = new TeamMember
            {
                FirstName = teamLead.FirstName,
                LastName = teamLead.LastName,
                Country = teamLead.Country,
                Email = teamLead.Email,
                ApplicationUser = teamLead.ApplicationUser,
                ImageUrl = teamLead.ImageUrl,
            };
            await unitOfWork.TeamMembers.AddAsync(newTeamMember);
            await unitOfWork.CompleteAsync();

            var identityApplicationUser = await unitOfWork.IdentityApplicationUsers.GetAsync(u=> u.IdentityUserId == teamLead.ApplicationUserId);
            if (identityApplicationUser == null)
            {
                response.Message = "Update Role failed";
                response.Errors = new List<string> { "Some Thing Went Wrong" };
                return BadRequest(response);
            }

            identityApplicationUser.ApplicationUserId = newTeamMember.Id;
            unitOfWork.IdentityApplicationUsers.Update(identityApplicationUser);

            unitOfWork.TeamLeads.Delete(teamLead);
            await unitOfWork.CompleteAsync();

            var responseData = new UpdateToTeamleadResponseDto
            {
                FirstName = newTeamMember.FirstName,
                LastName = newTeamMember.LastName,
                Email = newTeamMember.Email,
                NewId = newTeamMember.Id,
                ImageUrl = newTeamMember.ImageUrl
            };

            response.Success = true;
            response.Message = "Role Updated Successfully";
            response.Data = responseData;
            return Ok(response);
        }

    }
}
