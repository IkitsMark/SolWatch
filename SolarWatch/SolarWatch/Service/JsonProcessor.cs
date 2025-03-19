using System.Text.Json;
using SolarWatch.Model;

namespace SolarWatch.Service;

public class JsonProcessor : IJsonProcessor
{
    //takes JSON string (data) as input and attempts to parse it into a JsonDocument object.
    //If JSON is invalid, catces the JsonException and throws an ArgumentException with the message "Invalid JSON data."
    private JsonDocument ParseJson(string data)
    {
        try
        {
            return JsonDocument.Parse(data);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Invalid JSON data.", ex);
        }
    }
    //Get the sunrise time from the JSON data
    //Parse the JSON data using ParseJson method into a JsonDocument object
    //Get the "results" and "sunrise" properties from the root element using TryGetPropety
    //Return the "sunrise" property as a DateTime object
    public DateTime GetSunrise(string data)
    {
        using var json = ParseJson(data);
        if (json.RootElement.TryGetProperty("results", out var results) 
            && results.TryGetProperty("sunrise", out var sunrise))
        {
            return sunrise.GetDateTime();
        }
        throw new ArgumentException("Invalid JSON data: missing 'results' or 'sunset' property.");
    }
    //Get the sunset time from the JSON data
    //Parse the JSON data using ParseJson method into a JsonDocument object
    //Get the "results" & "sunset" property from the root element using TrygetProperty 
    //Return the "sunset" property as a DateTime object
    public DateTime GetSunset(string data)
    {
        using var json = ParseJson(data);
        if (json.RootElement.TryGetProperty("results", out var results) &&
            results.TryGetProperty("sunset", out var sunset))
        {
            return sunset.GetDateTime();
        }
        throw new ArgumentException("Invalid JSON data: missing 'results' or 'sunset' property.");
    }
    //Convert the JSON data to a Coordinate object
    //Parse the JSON data using ParseJson method into a JsonDocument object
    //Get the "lat" and "lon" properties from the root element
    //Return a new Coordinate object with the latitude and longitude values or throw an exception if the properties are missing
    public Coordinate ConvertDataToCoordinate(string data)
    {
        using var json = ParseJson(data);
        if (json.RootElement[0].TryGetProperty("lat", out var lat) &&
            json.RootElement[0].TryGetProperty("lon", out var lon))
        {
            return new Coordinate(lat.GetDouble(), lon.GetDouble());
        }
        throw new ArgumentException("Invalid JSON data: missing 'lat' or 'lon' property.");
    }
    //Convert the JSON data to a City object
    //Parse the JSON data using ParseJson method into a JsonDocument object
    //use ConvertDataToCoordinate method to get the latitude and longitude values
    //Get the "name", "country", and "state" properties from the root element
    //If the "state" property is not present, set it to null
    //Return a new City object with the name, latitude, longitude, country, and state values
    public City ConvertDataToCity(string data)
    {
        using var json = ParseJson(data);
        var coordinate = ConvertDataToCoordinate(data);

        var root = json.RootElement[0];
        var name = root.GetProperty("name").GetString();
        var country = root.GetProperty("country").GetString();
        var state = root.TryGetProperty("state", out var stateElement) ? stateElement.GetString() : null;

        if (name == null || country == null)
        {
            throw new ArgumentException("Invalid JSON data: missing 'name' or 'country' property.");
        }

        return new City(name, coordinate.Latitude, coordinate.Longitude, country, state);
    }

}