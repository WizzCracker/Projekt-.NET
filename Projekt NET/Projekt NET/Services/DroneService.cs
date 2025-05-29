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

        /*
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

            var currentDistrict = allDistricts.FirstOrDefault(d => GeoFunctions.IsPointInDistrict(d.BoundingPoints, currentPosition));
            if (currentDistrict == null)
                throw new InvalidOperationException("Dron nie znajduje się w żadnym dystrykcie.");

            if (GeoFunctions.IsPointInDistrict(currentDistrict.BoundingPoints, targetPosition))
            {
                // Lot wewnątrz dystryktu - jeden flight do celu
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

                // Lot pierwszego drona do punktu przecięcia
                var flight1 = new Flight
                {
                    DroneId = droneId,
                    DeliveryCoordinates = intersection
                };
                context.Flights.Add(flight1);
                await context.SaveChangesAsync();

                var firstDroneMoveTask = MoveDroneAsync(droneId, intersection.Latitude, intersection.Longitude);
               
                // Znajdź dystrykt docelowy i drona w nim
                var nextDistrict = allDistricts.FirstOrDefault(d => d.DistrictId != currentDistrict.DistrictId && GeoFunctions.IsPointInDistrict(d.BoundingPoints, intersection));
                if (nextDistrict == null)
                    throw new InvalidOperationException("Nie znaleziono dystryktu dla punktu przecięcia.");

                var activeDrones = await context.Drones
                    .Include(d => d.Model)
                    .Where(d => d.Status == DStatus.Active)
                    .ToListAsync();

                var secondDrone = activeDrones
                    .FirstOrDefault(d => GeoFunctions.IsPointInDistrict(nextDistrict.BoundingPoints, d.Coordinate));

                if (secondDrone == null)
                    throw new InvalidOperationException("Brak dostępnego drona w następnym dystrykcie.");

                // Lot drugiego drona od punktu przecięcia do celu
                var flight2 = new Flight
                {
                    DroneId = secondDrone.DroneId,
                    DeliveryCoordinates = targetPosition
                };
                context.Flights.Add(flight2);

                await context.SaveChangesAsync();

                var secondDroneMoveTask = MoveDroneAsync(secondDrone.DroneId, targetLat, targetLng);

                var results = await Task.WhenAll(firstDroneMoveTask, secondDroneMoveTask);

                return results.All(r => r);
            }
        }*/




        
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
                // Lot wewnątrz dystryktu - jeden flight do celu
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

                // Lot pierwszego drona do punktu przecięcia
                var flight1 = new Flight
                {
                    DroneId = droneId,
                    DeliveryCoordinates = intersection
                };
                context.Flights.Add(flight1);
                await context.SaveChangesAsync();

                var firstDroneMoveTask = MoveDroneAsync(droneId, intersection.Latitude, intersection.Longitude);

                // Znajdź dystrykt docelowy i drona w nim
                var nextDistrict = allDistricts.FirstOrDefault(d => d.DistrictId != currentDistrict.DistrictId && GeoFunctions.IsPointInDistrict(d.BoundingPoints, intersection));
                if (nextDistrict == null)
                    throw new InvalidOperationException("Nie znaleziono dystryktu dla punktu przecięcia.");

                var activeDrones = await context.Drones
                    .Include(d => d.Model)
                    .Where(d => d.Status == DStatus.Active)
                    .ToListAsync();

                var secondDrone = activeDrones
                    .FirstOrDefault(d => GeoFunctions.IsPointInDistrict(nextDistrict.BoundingPoints, d.Coordinate));

                if (secondDrone == null)
                    throw new InvalidOperationException("Brak dostępnego drona w następnym dystrykcie.");

                var intersection2 = GeoFunctions.FindIntersectionWithDistrictEdge(nextDistrict.BoundingPoints, secondDrone.Coordinate, intersection);
                if (intersection2 == null)
                    throw new InvalidOperationException("Nie udało się znaleźć punktu przecięcia z granicą dystryktu.");
                intersection2.Latitude = Math.Round(intersection2.Latitude,6);
                intersection2.Longitude = Math.Round(intersection2.Longitude,6);
                // Lot drugiego drona od punktu przecięcia do celu
                var flight2 = new Flight
                {
                    DroneId = secondDrone.DroneId,
                    DeliveryCoordinates = intersection2
                };
                context.Flights.Add(flight2);

                await context.SaveChangesAsync();

                var secondDroneMoveTask = MoveDroneAsync(secondDrone.DroneId, intersection2.Latitude, intersection2.Longitude);

                var results = await Task.WhenAll(firstDroneMoveTask, secondDroneMoveTask);

                return results.All(r => r);
            }
        }

        /*

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

            // Znajdź currentDistrict na podstawie DistrictId przypisanego do DroneCloud, którego jest częścią dron
            var currentDistrict = allDistricts.FirstOrDefault(d =>
                d.DistrictId == context.DroneClouds
                    .Where(dc => dc.DroneCloudId == drone.DroneCloudId)
                    .Select(dc => dc.DistrictId)
                    .FirstOrDefault()
            );

            if (currentDistrict == null)
                throw new InvalidOperationException("DronCloud nie jest przypisany do żadnego dystryktu.");

            if (GeoFunctions.IsPointInDistrict(currentDistrict.BoundingPoints, targetPosition))
            {
                // Lot wewnątrz dystryktu - jeden flight do celu
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

                // Lot pierwszego drona do punktu przecięcia
                var flight1 = new Flight
                {
                    DroneId = droneId,
                    DeliveryCoordinates = intersection
                };
                context.Flights.Add(flight1);
                await context.SaveChangesAsync();

                var firstDroneMoveTask = MoveDroneAsync(droneId, intersection.Latitude, intersection.Longitude);

                // Znajdź dystrykt docelowy - inny niż currentDistrict, który zawiera punkt intersection
                var nextDistrict = allDistricts.FirstOrDefault(d =>
                    d.DistrictId != currentDistrict.DistrictId &&
                    GeoFunctions.IsPointInDistrict(d.BoundingPoints, intersection)
                );

                if (nextDistrict == null)
                    throw new InvalidOperationException("Nie znaleziono dystryktu dla punktu przecięcia.");

                // Pobierz wszystkie aktywne drony
                var activeDrones = await context.Drones
                    .Include(d => d.Model)
                    .Where(d => d.Status == DStatus.Active)
                    .ToListAsync();

                // Wybierz drugiego drona, który jest przypisany do DroneCloud należącego do nextDistrict
                var secondDrone = activeDrones.FirstOrDefault(d =>
                {
                    var droneCloudDistrictId = context.DroneClouds
                        .Where(dc => dc.DroneCloudId == d.DroneCloudId)
                        .Select(dc => dc.DistrictId)
                        .FirstOrDefault();

                    return droneCloudDistrictId == nextDistrict.DistrictId;
                });

                if (secondDrone == null)
                    throw new InvalidOperationException("Brak dostępnego drona w następnym dystrykcie.");

                // Znajdź punkt przecięcia trasy drugiego drona z granicą jego dystryktu,
                // gdzie startem jest aktualna pozycja drugiego drona,
                // a celem jest intersection pierwszego drona
                var secondDroneIntersection = GeoFunctions.FindIntersectionWithDistrictEdge(nextDistrict.BoundingPoints, secondDrone.Coordinate, intersection);

                Coordinate secondDroneTarget;
                if (secondDroneIntersection != null)
                {
                    secondDroneTarget = secondDroneIntersection;
                }
                else
                {
                    // Jeśli przecięcie nie znalezione, to leci bezpośrednio do intersection (może już jest w dystrykcie)
                    secondDroneTarget = intersection;
                }

                // Stwórz lot dla drugiego drona do wyliczonego punktu
                var flight2 = new Flight
                {
                    DroneId = secondDrone.DroneId,
                    DeliveryCoordinates = secondDroneTarget
                };
                context.Flights.Add(flight2);

                await context.SaveChangesAsync();

                // Drugi dron leci do wyliczonego punktu przecięcia (lub intersection)
                var secondDroneMoveTask = MoveDroneAsync(secondDrone.DroneId, secondDroneTarget.Latitude, secondDroneTarget.Longitude);

                // Czekaj na oba ruchy
                var results = await Task.WhenAll(firstDroneMoveTask, secondDroneMoveTask);

                return results.All(r => r);
            }
        }*/


    }




}
