using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarWatch.Model;
using SolarWatch.Service.Repositories;
using SolarWatch.Service;

namespace SolarWatch.Controller;

[ApiController]
[Route("/api/[controller]")]
public class SunriseSunsetController : ControllerBase
{
    private readonly ILogger<SunriseSunsetController> _logger;
    private readonly IJsonProcessor _jsonProcessor;
    private readonly IOpenWeather _openWeather;
    private readonly ISunsetAndSunriseDataProvider _sunriseSunsetApi;
    private readonly ICityRepository _cityRepository;
    private readonly ISolarWatchRepository _solarWatchRepository;

    public SunriseSunsetController(IJsonProcessor jsonProcessorForSunrise, ILogger<SunriseSunsetController> logger, IOpenWeather openweather, ISunsetAndSunriseDataProvider sunriseSunsetApi, ICityRepository cityRepository, ISolarWatchRepository solarWatchRepository)
    {
        _jsonProcessor = jsonProcessorForSunrise;
        _logger = logger;
        _openWeather = openweather;
        _sunriseSunsetApi = sunriseSunsetApi;
        _cityRepository = cityRepository;
        _solarWatchRepository = solarWatchRepository;
    }

    [HttpGet("GetSunrise"), Authorize(Roles = "User, Admin")]
    public async Task<ActionResult<DateTime>> GetSunrise([FromQuery, Required] string city, [FromQuery, Required] string timeZone, [FromQuery] DateTime? date = null)
    {
        var actionResult = await getSunriseSunsetData("sunrise", city, timeZone, date);
        return actionResult;
    }

    [HttpGet("GetSunset"), Authorize(Roles = "User, Admin")]
    public async Task<ActionResult<DateTime>> GetSunset([FromQuery, Required] string city, [FromQuery, Required] string timeZone, [FromQuery] DateTime? date = null)
    {
        var actionResult = await getSunriseSunsetData("sunset", city, timeZone, date);
        return actionResult;
    }

    private async Task<ActionResult<DateTime>> getSunriseSunsetData(string type, string city, string timeZone, DateTime? date)
    {
        var cityFromDb = await _cityRepository.GetByNameAsync(city);

        if (cityFromDb != null)
        {
            var solarData = await _solarWatchRepository.GetSolarWatchAsync(cityFromDb.Id, date, timeZone);
            Console.WriteLine(solarData);

            if (solarData != null)
            {
                return Ok(type == "sunrise" ? solarData.Sunrise : solarData.Sunset);
            }
        }
        try
        {
            DateTime? sunriseOrSunsetDate = date;
            timeZone = Uri.UnescapeDataString(timeZone);
            var geocodingResponse = await _openWeather.GetGeoCodeFromApi(city);
            Coordinate coordinateForCity =
                _jsonProcessor.ConvertDataToCoordinate(geocodingResponse);
            var cityToAdd = _jsonProcessor.ConvertDataToCity(geocodingResponse);
            int cityIdToAdd;

            if (cityFromDb == null)
            {
                await _cityRepository.AddAsync(cityToAdd);
                cityIdToAdd = cityToAdd.Id;
            }
            else
            {
                cityIdToAdd = cityFromDb.Id;
            }

            var sunriseSunsetData = await _sunriseSunsetApi.GetSunriseAndSunset(coordinateForCity, timeZone, sunriseOrSunsetDate);

            var sunrise = _jsonProcessor.GetSunrise(sunriseSunsetData);
            var sunset = _jsonProcessor.GetSunset(sunriseSunsetData);

            var solarData = new SolarData(sunrise, sunset, cityIdToAdd, timeZone, sunriseOrSunsetDate ?? DateTime.Today) 
            { 
                TimeZone = timeZone
            };

            await _solarWatchRepository.AddAsync(solarData);


            return Ok(type == "sunrise" ? sunrise : sunset);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, type == "sunrise" ? "Error getting sunrise data" : "Error getting sunset data");
            return NotFound(type == "sunrise" ? "Error getting sunrise data" : "Error getting sunset data");
        }
    }
}