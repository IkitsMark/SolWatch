// 

using System.Text.Json;
using SolarWatch.Model;

namespace SolarWatch.Service;

public class JsonProcessor : IJsonProcessor
{
    public string[]? ProcessLongitudeAndLatitude(string data)
    {
        JsonDocument json = JsonDocument.Parse(data);
        var lat = json.RootElement.GetProperty("lat")[0].GetRawText();
        var lon = json.RootElement.GetProperty("lon")[0].GetRawText();

        var positionData = new[] { lat, lon };
        
        return positionData;
    }
    
    private static DateTime GetDateTimeFromUnixTimeStamp(long timeStamp)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timeStamp);
        DateTime dateTime = dateTimeOffset.UtcDateTime;

        return dateTime;
    }
    
}

/*public DateOnly Date { get; set; }

public DateTime Sunrise { get; set; }

public DateTime Sunset { get; set; }
    
public string? Summary { get; set; }*/