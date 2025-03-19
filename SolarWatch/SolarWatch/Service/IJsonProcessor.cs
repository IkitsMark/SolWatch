using SolarWatch.Model;
using System.Text.Json;

namespace SolarWatch.Service;

public interface IJsonProcessor
{
    public DateTime GetSunrise(string data);
    public DateTime GetSunset(string data);
    public Coordinate ConvertDataToCoordinate(string data);
    public City ConvertDataToCity(string data);
}