namespace SolarWatch.Service
{
    public interface IOpenWeather
    {
        public Task<string> GetGeoCodeFromApi(string cityName);
    }
}
