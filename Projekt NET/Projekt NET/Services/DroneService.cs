using Projekt_NET.Models.System;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Projekt_NET.Models;

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
            return DroneMoveManager.TryMoveDroneAsync(droneId, async (cancellationToken) =>
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DroneDbContext>();

                var drone = await context.Drones
                    .Include(d => d.Model)
                    .FirstOrDefaultAsync(d => d.DroneId == droneId);

                if (drone != null)
                {
                    await drone.MoveToAsync(latitude, longitude, context, cancellationToken);
                }
            });
        }


        public async Task<bool> StopDrone(int droneId)
        {
            
            if (!DroneMoveManager.TryStopDrone(droneId))
                return false;

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DroneDbContext>();
            var drone = await context.Drones.FirstOrDefaultAsync(d => d.DroneId == droneId);
            if (drone == null)
                return false;

            var flight = await context.Flights.FirstOrDefaultAsync(f => f.DroneId == droneId && f.ArrivDate == null);

            if (flight != null)
            {
                flight.ArrivDate = DateTime.UtcNow;
                context.Update(flight);
            }
            drone.Status = DStatus.Active;
            context.Update(drone);

            await context.SaveChangesAsync();
            return true;
        }

        public (Coordinate adjustedCoordinate, bool wasClipped, string? errorMessage) AdjustCoordinateToDistrictBoundary(
        List<Coordinate> districtBoundary,
        Coordinate currentPosition,
        Coordinate targetPosition)
        {
            if (GeoFunctions.IsPointInDistrict(districtBoundary, targetPosition))
            {
                return (targetPosition, false, null);
            }

            var intersection = GeoFunctions.FindIntersectionWithDistrictEdge(districtBoundary, currentPosition, targetPosition);

            if (intersection == null)
            {
                return (null, false, "Cannot find intersection with district boundary");
            }

            return (intersection, true, null);
        }

        
        




        
        public async Task<bool> HandleCrossDistrictFlightAsync(int droneId, double targetLat, double targetLng)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DroneDbContext>();

            var drone = await context.Drones.Include(d => d.Model).FirstOrDefaultAsync(d => d.DroneId == droneId);
            if (drone == null) return false;

            var allDistricts = await context.Districts
                .Include(d => d.BoundingPoints)
                .ToListAsync();

            var currentPosition = drone.Coordinate;
            var targetPosition = new Coordinate { Latitude = targetLat, Longitude = targetLng };

            var currentDistrict = allDistricts.FirstOrDefault(d =>
                d.DistrictId == context.DroneClouds
                    .Where(dc => dc.DroneCloudId == drone.DroneCloudId)
                    .Select(dc => dc.DistrictId)
                    .FirstOrDefault()
            );
            if (currentDistrict == null)
                throw new InvalidOperationException("Dron nie znajduje się w żadnym dystrykcie.");

            if (GeoFunctions.IsPointInDistrict(currentDistrict.BoundingPoints, targetPosition))
            {
                var flight = new Flight
                {
                    DroneId = droneId,
                    DeliveryCoordinates = targetPosition
                };
                context.Flights.Add(flight);
                await context.SaveChangesAsync();

                return await MoveDroneAsync(droneId, targetLat, targetLng);
            }
            else
            {
                var intersection = GeoFunctions.FindIntersectionWithDistrictEdge(currentDistrict.BoundingPoints, currentPosition, targetPosition);
                if (intersection == null)
                    throw new InvalidOperationException("Nie udało się znaleźć punktu przecięcia z granicą dystryktu.");


                var flight1 = new Flight
                {
                    DroneId = droneId,
                    DeliveryCoordinates = intersection
                };
                context.Flights.Add(flight1);
                await context.SaveChangesAsync();

                var firstDroneMoveTask = MoveDroneAsync(droneId, intersection.Latitude, intersection.Longitude);

                var districtCandidate = allDistricts
                    .Where(d => d.DistrictId != currentDistrict.DistrictId)
                    .FirstOrDefault(d => GeoFunctions.IsPointInDistrict(d.BoundingPoints, GeoFunctions.MovePointTowards(intersection, GeoFunctions.CalculateCentroid(d.BoundingPoints), 100)));

                if (districtCandidate == null)
                {
                    // Fallback: znajdź dystrykt najbliższy do punktu intersection
                    districtCandidate = allDistricts
                        .Where(d => d.DistrictId != currentDistrict.DistrictId)
                        .OrderBy(d => GeoFunctions.DistanceToDistrictBoundary(d.BoundingPoints, intersection))
                        .FirstOrDefault();

                    if (districtCandidate == null)
                        throw new InvalidOperationException("Nie znaleziono dystryktu dla punktu przecięcia ani najbliższego dystryktu.");
                }

                var nextDistrict = districtCandidate;

                if (nextDistrict == null)
                    throw new InvalidOperationException("Nie znaleziono dystryktu dla punktu przecięcia.");

                // Pobierz wszystkie aktywne drony oprócz pierwszego
                var activeDrones = await context.Drones
                    .Include(d => d.Model)
                    .Include(d => d.DroneCloud)  // uwzględnij powiązanie z DroneCloud
                    .Where(d => d.Status == DStatus.Active && d.DroneId != droneId)
                    .ToListAsync();

                // Wybierz drugiego drona, który jest przypisany do nextDistrict przez DroneCloud
                var secondDrone = activeDrones
                    .FirstOrDefault(d => d.DroneCloud != null && d.DroneCloud.DistrictId == nextDistrict.DistrictId);

                if (secondDrone == null)
                    throw new InvalidOperationException("Brak dostępnego drona w następnym dystrykcie.");



                
                var rawIntersection = GeoFunctions.FindIntersectionWithDistrictEdge(nextDistrict.BoundingPoints, secondDrone.Coordinate, intersection);
                if (rawIntersection == null)
                    throw new InvalidOperationException("Nie udało się znaleźć punktu przecięcia z granicą dystryktu.");

                var districtCenter = GeoFunctions.CalculateCentroid(nextDistrict.BoundingPoints);

                var intersection2 = GeoFunctions.MovePointTowards(rawIntersection, districtCenter, 1000);


                var flight2 = new Flight
                {
                    DroneId = secondDrone.DroneId,
                    DeliveryCoordinates = intersection2
                };
                context.Flights.Add(flight2);

                await context.SaveChangesAsync();

                var secondDroneMoveTask = MoveDroneAsync(secondDrone.DroneId, intersection2.Latitude, intersection2.Longitude);

                await Task.WhenAll(firstDroneMoveTask, secondDroneMoveTask);

                return await HandleCrossDistrictFlightAsync(secondDrone.DroneId, targetLat, targetLng);
            }
        }

        


    }




}
