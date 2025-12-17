using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SynkTask.DataAccess.IConfiguration;
using SynkTask.Models;
using SynkTask.Models.DTOs;

namespace SynkTask.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;

        public CountryController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("All")]
        [ProducesResponseType<ApiResponse<List<string>>>(200)]
        public async Task<IActionResult> GetAll()
        {
            var response = new ApiResponse<List<string>>() { Success = true };
            var countries= await unitOfWork.Countries.GetAllAsync(c=> c.IsAvailable);
            if (!countries.Any())
            {
                response.Message = "No Countries Found";
                return Ok(response);
            }
            response.Message = " Countries Retrieved Successfully";
            response.Data = countries.Select(c=>c.Name).ToList();

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType<ApiResponse<Country>>(200)]
        public async Task<IActionResult> AddCountry([FromBody]string countryName)
        {
            var response = new ApiResponse<Country>();

            if (countryName == null)
            {
                response.Message = "countryName is Must.";
                return BadRequest(response);
            }

            var newCountry = new Country { Name =  countryName , IsAvailable = true};
            await unitOfWork.Countries.AddAsync(newCountry);
            await unitOfWork.CompleteAsync();

            response.Success = true;
            response.Message = "Country Added Successfully";
            response.Data = newCountry;

            return Ok(response);
        }


        [HttpGet("{countryName:alpha}/{isAvalibale:bool}")]
        [ProducesResponseType<ApiResponse<Country>>(200)]
        public async Task<IActionResult> ChangeCountryState(string countryName, bool isAvalibale)
        {
            var response = new ApiResponse<Country>() ;

            if (countryName == null)
            {
                response.Message = "countryName is Must.";
                return BadRequest(response);
            }

            var country = await unitOfWork.Countries.GetAsync(c=> c.Name == countryName);
            if (country == null)
            {
                response.Message = "country is not found or incorrect.";
                return NotFound(response);
            }

            country.IsAvailable = isAvalibale;
            unitOfWork.Countries.Update(country);
            await unitOfWork.CompleteAsync();

            response.Success = true;
            response.Message = "Country Avalibility Updated Successfully";
            response.Data = country;

            return Ok(response);
        }
    }
}
