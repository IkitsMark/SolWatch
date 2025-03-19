using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using SolarWatch.Data;
using SolarWatch.Model;
using SolarWatch.Service;
using SolarWatch.Service.Repositories;
using System.ComponentModel.DataAnnotations;

namespace SolarWatch.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class SolarWatchController : ControllerBase
{
    
    private readonly ILogger<SolarWatchController> _logger;
    private readonly ISolarWatchRepository _solarWatchRepository;
    
    public SolarWatchController(ISolarWatchRepository solarWatchRepository, ILogger<SolarWatchController> logger)
    {
        _logger = logger;
        _solarWatchRepository = solarWatchRepository;
    }

    [HttpGet("GetAllSolarWatchData"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<SolarData>>> GetAllSolarWatchData()
    {
        try
        {
            var solarData = await _solarWatchRepository.GetAllSolarWatchAsync();
            if (solarData == null || !solarData.Any())
            { 
                return NotFound("No solar data found!");
            }
            return Ok(solarData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all solar data");
            return NotFound("Error getting all solar data");
        }
    }

    [HttpPut("UpdateSolarData"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<SolarData>> UpdateSolarData([FromBody, Required] SolarData newData)
    {
        try
        {
            await _solarWatchRepository.UpdateAsync(newData);

            return Ok(newData);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating Solar Data");
            return NotFound("Error updating Solar Data");
        }
    }

    [HttpDelete("DeleteSolarData"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> DeleteSolarData(int id)
    {
        try
        {
            await _solarWatchRepository.DeleteAsync(id);
            return Ok(id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deleting Solar Data");
            return NotFound("Error deleting Solar Data");
        }
    }
}