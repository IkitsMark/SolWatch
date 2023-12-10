// 

namespace SolarWatch.Service;

public interface ISunsetAndSunriseDataProvider
{
    Task<string> GetCurrent(double lat, double lon);
}