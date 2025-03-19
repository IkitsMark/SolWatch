using SolarWatch.Model;
using System.Net;
using System.Text.Json;

namespace SolarWatch.Service;

public class SunriseAndSunsetApi : ISunsetAndSunriseDataProvider
{
    private readonly ILogger<SunriseAndSunsetApi> _logger;
    public SunriseAndSunsetApi(ILogger<SunriseAndSunsetApi> logger)
    {
        _logger = logger;
    }

    public async Task<string> GetSunriseAndSunset(Coordinate coordinate, string timeZone, DateTime? date)
    {
        //Check if the latitute and longitude is possible
        if (coordinate.Latitude < -90 || coordinate.Latitude > 90 || coordinate.Longitude < -180 || coordinate.Longitude > 180)
        {
            throw new ArgumentException("Invalid input data. Please check coordinates and timezone.");
        }

        var url = generateURL(coordinate, timeZone, date);
        using var client = new HttpClient();
        HttpResponseMessage response;
        JsonDocument document;

        try
        {
            _logger.LogInformation("Calling Sunrise-Sunset API with url: {url}", url);

            response = await client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            document = JsonDocument.Parse(responseString);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred during Sunrise-Sunset API call.");
            throw;
        }

        var root = document.RootElement;

        if (!root.EnumerateObject().Any())
        {
            _logger.LogError("Solar API: No valid data found for coordinates '{coordinate.Latitude}, {coordinate.Longitude}', and timezone: {timeZone}", coordinate.Latitude, coordinate.Longitude, timeZone);
            throw new ArgumentException("Invalid input data. Please check coordinates and timezone.");
        }
        return await response.Content.ReadAsStringAsync();
    }


    private string generateURL(Coordinate coordinate, string timeZone, DateTime? date)
    {
        // Format date or use "today" if null
        var dateString = date?.ToString("yyyy-MM-dd") ?? "today"; 
        return $"https://api.sunrise-sunset.org/json?lat={coordinate.Latitude}&lng={coordinate.Longitude}&date={dateString}&tzid={timeZone}&formatted=0";
    }
}