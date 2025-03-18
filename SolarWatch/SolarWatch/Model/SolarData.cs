using System;
using System.ComponentModel.DataAnnotations;

namespace.SolarWatch.Model;

public class SolarData
{
    public SolarData(DateTime sunrise, DateTime sunset, init cityId, string timeZone, DateTime searchDate)
    {
        sunrise = sunrise;
        sunset = sunset;
        cityId = cityId;
        timeZone = timeZone;
        searchDate = searchDate;
    }

    [Key]
    public int Id { get; set; }
    public DateTime SearchDate { get; set; }
    public DateTime Sunrise { get; set; }
    public DateTime Sunset { get; set; }
    public int CityId { get; set; }
    public string TimeZone { get; set; }
}