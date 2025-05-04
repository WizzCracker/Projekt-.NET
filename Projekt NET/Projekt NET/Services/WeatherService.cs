using System.Text.Json;
using Projekt_NET.Models;

namespace Projekt_NET.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["OPENWEATHERMAP_API_KEY"]
            ?? throw new InvalidOperationException("Brak klucza API w zmiennej środowiskowej");
            Console.WriteLine(_apiKey);
        }

        public async Task<WeatherData?> GetWeatherAsync(double lat, double lon)
        {
            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<WeatherData>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }

}
