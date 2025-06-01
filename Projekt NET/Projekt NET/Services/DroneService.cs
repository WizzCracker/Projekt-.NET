using Projekt_NET.Models.System;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Projekt_NET.Models;
using Projekt_NET.Migrations;

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








        public async Task<bool> HandleCrossDistrictFlightAsync(int droneId, double targetLat, double targetLng, int? packageId = null)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DroneDbContext>();

            var flightPath = new FlightPath
            {
                FlightList = new List<Flight>()
            };

            context.FlightPaths.Add(flightPath);


            async Task<bool> HandleRecursive(int currentDroneId, Coordinate start, Coordinate destination)
            {
                var drone = await context.Drones
                    .Include(d => d.Model)
                    .Include(d => d.Coordinate)
                    .FirstOrDefaultAsync(d => d.DroneId == currentDroneId);
                if (drone == null) return false;

                var allDistricts = await context.Districts
                    .Include(d => d.BoundingPoints)
                    .ToListAsync();

                var currentDistrict = allDistricts.FirstOrDefault(d =>
                    d.DistrictId == context.DroneClouds
                        .Where(dc => dc.DroneCloudId == drone.DroneCloudId)
                        .Select(dc => dc.DistrictId)
                        .FirstOrDefault());

                if (currentDistrict == null)
                    throw new InvalidOperationException("Dron nie znajduje się w żadnym dystrykcie.");

                if (GeoFunctions.IsPointInDistrict(currentDistrict.BoundingPoints, destination))
                {
                    var flight = new Flight
                    {
                        DroneId = drone.DroneId,
                        DeliveryCoordinates = destination,
                        DepDate = DateTime.UtcNow,
                    };
                    context.Flights.Add(flight);
                    flightPath.FlightList.Add(flight);
                    await context.SaveChangesAsync();

                    await MoveDroneAsync(drone.DroneId, destination.Latitude, destination.Longitude);
                    return true;
                }

                var intersection = GeoFunctions.FindIntersectionWithDistrictEdge(currentDistrict.BoundingPoints, start, destination);
                if (intersection == null)
                    throw new InvalidOperationException("Nie udało się znaleźć punktu przecięcia z granicą dystryktu.");

                var flight1 = new Flight
                {
                    DroneId = drone.DroneId,
                    DeliveryCoordinates = intersection,
                    DepDate = DateTime.UtcNow,
                };
                context.Flights.Add(flight1);
                flightPath.FlightList.Add(flight1);
                await context.SaveChangesAsync();

                var moveTask1 = MoveDroneAsync(drone.DroneId, intersection.Latitude, intersection.Longitude);

                var districtCandidate = allDistricts
                    .Where(d => d.DistrictId != currentDistrict.DistrictId)
                    .FirstOrDefault(d => GeoFunctions.IsPointInDistrict(d.BoundingPoints, GeoFunctions.MovePointTowards(intersection, GeoFunctions.CalculateCentroid(d.BoundingPoints), 100)))
                    ?? allDistricts
                        .Where(d => d.DistrictId != currentDistrict.DistrictId)
                        .OrderBy(d => GeoFunctions.DistanceToDistrictBoundary(d.BoundingPoints, intersection))
                        .FirstOrDefault();

                if (districtCandidate == null)
                    throw new InvalidOperationException("Nie znaleziono następnego dystryktu.");

                var secondDrone = await context.Drones
                    .Include(d => d.Model)
                    .Include(d => d.Coordinate)
                    .Include(d => d.DroneCloud)
                    .Where(d => d.Status == DStatus.Active && d.DroneId != drone.DroneId)
                    .FirstOrDefaultAsync(d => d.DroneCloud != null && d.DroneCloud.DistrictId == districtCandidate.DistrictId);

                if (secondDrone == null)
                    throw new InvalidOperationException("Brak dostępnego drona w następnym dystrykcie.");

                var rawIntersection = GeoFunctions.FindIntersectionWithDistrictEdge(districtCandidate.BoundingPoints, secondDrone.Coordinate, intersection);
                if (rawIntersection == null)
                    throw new InvalidOperationException("Nie udało się znaleźć drugiego punktu przecięcia.");

                var intersection2 = GeoFunctions.MovePointTowards(rawIntersection, GeoFunctions.CalculateCentroid(districtCandidate.BoundingPoints), 1000);

                var flight2 = new Flight
                {
                    DroneId = secondDrone.DroneId,
                    DeliveryCoordinates = intersection2,
                    DepDate = DateTime.UtcNow
                };
                context.Flights.Add(flight2);
                flightPath.FlightList.Add(flight2);
                await context.SaveChangesAsync();

                var moveTask2 = MoveDroneAsync(secondDrone.DroneId, intersection2.Latitude, intersection2.Longitude);

                var distance = GeoFunctions.HaversineDistance(
                    drone.Coordinate.Latitude, drone.Coordinate.Longitude,
                    secondDrone.Coordinate.Latitude, secondDrone.Coordinate.Longitude);

                if (distance > drone.Range || distance > secondDrone.Range)
                    throw new InvalidOperationException("Drony za daleko.");

                await Task.WhenAll(moveTask1, moveTask2);

                return await HandleRecursive(secondDrone.DroneId, intersection2, destination);
            }

            var startCoord = (await context.Drones
                .Include(d => d.Coordinate)
                .FirstAsync(d => d.DroneId == droneId)).Coordinate;

            var destination = new Coordinate { Latitude = targetLat, Longitude = targetLng };

            var result = await HandleRecursive(droneId, startCoord, destination);

            if (packageId.HasValue)
            {
                var delivery = new Delivery
                {
                    PackageId = packageId.Value,
                    FlightPathId = flightPath.FlightPathId
                };
                context.Deliveries.Add(delivery);
                await context.SaveChangesAsync();

                var deliveryLog = new DeliveryLog
                {
                    DeliveryId = delivery.DeliveryId,
                    LogDate = DateTime.UtcNow,
                    Remarks = "Dostawa zakończona"
                };
                context.DeliveryLogs.Add(deliveryLog);
                await context.SaveChangesAsync();
            }

            return result;
        }





    }




}
