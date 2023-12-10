// 

using System.Net;

namespace SolarWatch.Service;

public class SunriseAndSunSetApi : ISunsetAndSunriseDataProvider
{
    private readonly ILogger<SunriseAndSunSetApi> _logger;

    public SunriseAndSunSetApi(ILogger<SunriseAndSunSetApi> logger)
    {
        _logger = logger;
    }
    
    public async Task<string> GetCurrent(double lat, double lon)
    {
        var url = $"https://api.sunrise-sunset.org/json?lat={lat}&lng={lon}";
        
        using var client = new HttpClient();
        _logger.LogInformation("Calling OpenWeather API with url: {url}", url);

        var response = await client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }
}