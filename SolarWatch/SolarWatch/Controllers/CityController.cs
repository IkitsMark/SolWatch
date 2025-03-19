using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.Model;
using SolarWatch.Service.Repositories;
using System.ComponentModel.DataAnnotations;

namespace SolarWatch.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class CityController : ControllerBase
    {
        private readonly ICityRepository _cityRepository;
        private readonly ILogger<CityController> _logger;

        public CityController(ILogger<CityController> logger, ICityRepository cityRepository)
        {
            _logger = logger;
            _cityRepository = cityRepository;
        }

        [HttpGet("GetByName"), Authorize(Roles = "Admin")]
        public async Task<ActionResult<City>> GetByName([FromQuery, Required] string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("City name is required");
            }

            try
            {
                var city = await _cityRepository.GetByNameAsync(name);
                if (city == null)
                {
                    return NotFound("City not found");
                }
                return Ok(city);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while getting city by name");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("GetById"), Authorize(Roles = "Admin")]
        public async Task<ActionResult<City>> GetById([FromQuery, Required] int id)
        {
            if (id <= 0)
            {
                return BadRequest("Valid city Id is required");
            }

            try
            {
                var city = await _cityRepository.GetByIdAsync(id);
                if (city == null)
                {
                    return NotFound("City not found");
                }
                return Ok(city);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting city from db");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("GetAllCities"), Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<City>>> GetAllCities()
        {
            try
            {
                var allCities = await _cityRepository.GetAllCitiesAsync();
                return Ok(allCities);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting all cities");
                return StatusCode(500,"Internal server error.");
            }
        }

        [HttpPut("UpdateCity"), Authorize(Roles = "Admin")]
        public async Task<ActionResult<City>> UpdateCity([FromBody, Required] City newCity)
        {
            try
            {
                await _cityRepository.UpdateAsync(newCity);
                return Ok(newCity);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating City");
                return StatusCode(500, "Internal server error.");
            }

        }

        [HttpDelete("DeleteCity"), Authorize(Roles = "Admin")]
        public async Task<ActionResult<int>> DeleteCity([FromQuery, Required] int id)
        {
            if (id <= 0)
            {
                return BadRequest("Valid city Id is required");
            }
            try
            {
                await _cityRepository.DeleteAsync(id);
                return Ok(id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deleting City");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("PostCityToDb"), Authorize(Roles = "Admin")]
        public async Task<ActionResult<SolarData>> PostCity(
            [FromQuery, Required] string country,
            [FromQuery, Required] string name,
            [FromQuery] string? state,
            [FromQuery, Required] double latitude,
            [FromQuery, Required] double longitude)
        {
            if (string.IsNullOrEmpty(country) || 
                string.IsNullOrEmpty(name) ||
                latitude == 0 ||
                longitude == 0)
            {
                return BadRequest("Invalid input parameters for country, name, latitude or longitude");
            }


            try
            {
                var newData = new City(name, latitude, longitude, country, state);
                await _cityRepository.AddAsync(newData);
                return Ok(newData);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error adding City");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
