// 

using SolarWatch.Model;

namespace SolarWatch.Service;

public interface IJsonProcessor
{
    string[] ProcessLongitudeAndLatitude(string data);
}