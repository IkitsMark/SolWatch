using System;
using System.ComponentModel.DataAnnotations;

namespace SolarWatch.Model;

public class SolarData
{
    public SolarData(DateTime sunrise, DateTime sunset, int cityId, string timeZone, DateTime searchDate)
    {
        Sunrise = sunrise;
        Sunset = sunset;
        CityId = cityId;
        TimeZone = timeZone;
        SearchDate = searchDate;
    }

    [Key]
    public int Id { get; set; }
    public DateTime SearchDate { get; set; }
    public DateTime Sunrise { get; set; }
    public DateTime Sunset { get; set; }
    public int CityId { get; set; }
    public required string TimeZone { get; set; }
}