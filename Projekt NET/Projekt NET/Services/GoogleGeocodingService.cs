using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Projekt_NET.Services
{
    public class GoogleGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GoogleGeocodingService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleMaps:ApiKey"];
        }

        public async Task<(double lat, double lng)?> GeocodeAsync(string address)
        {
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<GoogleGeocodingResponse>(content);

            if (data?.Results != null && data.Results.Count > 0)
            {
                var loc = data.Results[0].Geometry.Location;
                return (loc.Lat, loc.Lng);
            }

            return null;
        }
    }

    public class GoogleGeocodingResponse
    {
        public List<Result> Results { get; set; }

        public class Result
        {
            public Geometry Geometry { get; set; }
        }

        public class Geometry
        {
            public Location Location { get; set; }
        }

        public class Location
        {
            [JsonPropertyName("lat")]
            public double Lat { get; set; }

            [JsonPropertyName("lng")]
            public double Lng { get; set; }
        }
    }
}
