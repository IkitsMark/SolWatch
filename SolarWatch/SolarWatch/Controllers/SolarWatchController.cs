using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using SolarWatch.Data;
using SolarWatch.Model;
using SolarWatch.Service;
using SolarWatch.Service.Repositories;

namespace SolarWatch.Controllers;

[ApiController]
[Route("[controller]")]
public class SolarWatchController : ControllerBase
{
    //Should be refactored to use either the DB or the Repo, 
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    
    private readonly ILogger<SolarWatchController> _logger;

    private readonly IJsonProcessor _jsonProcessor;
    private readonly ISunsetAndSunriseDataProvider _sunsetProvider;
    private readonly ICityRepository _cityRepository;
    
    public SolarWatchController(ILogger<SolarWatchController> logger, IJsonProcessor processor, ISunsetAndSunriseDataProvider provider, ICityRepository cityRepository)
    {
        _logger = logger;
        _jsonProcessor = processor;
        _sunsetProvider = provider;
        _cityRepository = cityRepository;
    }

    /*[HttpGet(Name = "GetSunriseAndSunset"), Authorize(Roles="admin")]
    public async Task<ActionResult<SolarWatch>> Get(string cityName)
    {
        await using var dbContext = new SolarWatchContext();

        var city = dbContext.Cities.FirstOrDefault(c => c.Name == cityName);
        
        var apiKey = "f2f328e4e40ac894197f8af45ebc474a";
        var url = $"http://api.openweathermap.org/geo/1.0/direct?q={cityName}&appid={apiKey}";

        using var client = new HttpClient();

        _logger.LogInformation($"Calling API with {url}", url);

        var weatherData = await client.GetStringAsync(url);

        string[] lonAndLan = _jsonProcessor.ProcessLongitudeAndLatitude(weatherData);

        var newCity = new City();
        
        var lat = lonAndLan[0];
        var lon = lonAndLan[1];
        
        try
        {
            var solarData = await _sunsetProvider.GetCurrent(lat, lon);
            return Ok(_jsonProcessor.Process(solarData));
        }
        catch(Exception e)
        {
            _logger.LogError(e,"It's over...");
            return NotFound("Error getting data");
        }
    }*/
    
    [HttpGet("GetCurrent"), Authorize(Roles="User,Admin")]
    public async Task<ActionResult<SolarWatch>> GetCurrent(string cityName)
    {
        var city = _cityRepository.GetByName(cityName);
        if (city == null)
        {
            return NotFound($"City {cityName} not found. :(");
        }
        
        try
        {
            var weatherData = await _sunsetProvider.GetCurrent(city.Latitude, city.Longitude);
            return Ok(_jsonProcessor.ProcessLongitudeAndLatitude(weatherData));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting weather data");
            return NotFound("Error getting weather data");
        }
    }
}