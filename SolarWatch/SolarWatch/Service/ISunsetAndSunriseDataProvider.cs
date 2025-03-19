using SolarWatch.Model;

namespace SolarWatch.Service;

public interface ISunsetAndSunriseDataProvider
{
    public Task<string> GetSunriseAndSunset(Coordinate coordinate, string timeZone, DateTime? date);
}