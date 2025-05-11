using Projekt_NET.Models.System;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Projekt_NET.Services
{
    public class DroneService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public DroneService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<(double lat, double lng)?> GeocodeAddressAsync(string address)
        {
            var apiKey = _configuration["GoogleMaps:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Brak klucza Google Maps w konfiguracji.");

            var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={apiKey}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            var location = doc.RootElement
                .GetProperty("results")[0]
                .GetProperty("geometry")
                .GetProperty("location");

            double lat = location.GetProperty("lat").GetDouble();
            double lng = location.GetProperty("lng").GetDouble();

            return (lat, lng);
        }

        public Task<bool> MoveDroneAsync(int droneId, double latitude, double longitude)
        {
            return DroneMoveManager.TryMoveDroneAsync(droneId, async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DroneDbContext>();

                var drone = await context.Drones
                    .Include(d => d.Model)
                    .FirstOrDefaultAsync(d => d.DroneId == droneId);

                if (drone != null)
                {
                    await drone.MoveToAsync(latitude, longitude, context);
                }
            });
        }
    }

}
