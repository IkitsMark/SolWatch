using System.Text.Json;

namespace SolarWatch.Service
{
    public class OpenWeather : IOpenWeather
    {
        private readonly ILogger<OpenWeather> _logger;
        private readonly HttpClient _client;
        private readonly string _apiKey;

        public OpenWeather(ILogger<OpenWeather> logger, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _client = httpClient;
            _apiKey = configuration["OpenWeatherApikey"] ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<string> GetGeoCodeFromApi(string cityName)
        {
            var url = $"http://api.openweathermap.org/geo/1.0/direct?q={cityName}&limit=5&appid={_apiKey}";

            using var client = new HttpClient();
            try
            {
                var respone = await _client.GetAsync(url).ConfigureAwait(false);
                respone.EnsureSuccessStatusCode();

                var responseString = await respone.Content.ReadAsStringAsync().ConfigureAwait(false);

                using var document = JsonDocument.Parse(responseString);
                var root = document.RootElement;

                if (root.GetArrayLength() == 0)
                {
                    _logger.LogError("No valid data found for city: {cityName}", cityName);
                    throw new ArgumentException("Invalid input data. Please check city name.", nameof(cityName));
                }
                return responseString;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error occurred during OpenWeather API call.");
                throw;
            }

        }
    }
}
