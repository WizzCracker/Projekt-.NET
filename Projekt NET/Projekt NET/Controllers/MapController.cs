using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt_NET.Models; 
using Projekt_NET.Models.System;
using Projekt_NET.Services;

[Authorize]
public class MapController : Controller
{
    private readonly DroneDbContext _context;
    private readonly WeatherService _weatherService;
    private readonly DroneService _droneService;

    public MapController(DroneDbContext context, WeatherService weatherService, DroneService droneService)
    {
        _context = context;
        _weatherService = weatherService;
        _droneService = droneService;
    }

    public IActionResult Index()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENWEATHERMAP_API_KEY");
        ViewBag.OpenWeatherApiKey = apiKey;
        return View();
    }

    public IActionResult Weather()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENWEATHERMAP_API_KEY");
        ViewBag.OpenWeatherApiKey = apiKey;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetDroneData()
    {
        var drones = await _context.Drones
            .Include(d => d.Model)
            .Include(d => d.DroneCloud)
            .ToListAsync();

        var result = new List<object>();

        foreach (var d in drones)
        {
            double lat = d.Coordinate.Latitude;
            double lon = d.Coordinate.Longitude;

            var weather = await _weatherService.GetWeatherAsync(lat, lon);
            bool isGrounded = false;
            string reason = "";

            if (weather != null)
            {
                if (weather.Wind?.Speed >= 12.5)
                {
                    isGrounded = true;
                    reason += $"Silny wiatr ({weather.Wind.Speed} m/s). ";
                }

                if (weather.Weather?.Any(w => w.Main == "Rain" || w.Main == "Thunderstorm") == true)
                {
                    isGrounded = true;
                    reason += $"Zła pogoda: {weather.Weather[0].Main}. ";
                }

                if (weather.Rain?.OneHour > 0)
                {
                    isGrounded = true;
                    reason += $"Opady: {weather.Rain.OneHour} mm. ";
                }
            }

            int droneId = d.DroneId;
            string status = d.Status.ToString();

            var popupContent = $@"
            <strong>{d.CallSign}</strong><br>
            Status: {d.Status}<br>
            Model: {d.Model?.Name ?? "N/A"}<br>
            Range: {d.Range} m<br>
            Cloud: {d.DroneCloud?.Name ?? "Unassigned"}<br>";

            if (isGrounded)
            {
                popupContent += $"<span style='color:red;'><strong>UZIEMIONY:</strong> {reason}</span>";
            }

            result.Add(new
            {
                lat,
                lng = lon,
                isGrounded,
                droneId,
                status,
                popup = popupContent
            });
        }

        return Json(new { markers = result });
    }

    [HttpGet]
    public async Task<IActionResult> GetSingleDroneData(int droneId)
    {
        var d = await _context.Drones
            .Include(dr => dr.Model)
            .Include(dr => dr.DroneCloud)
            .FirstOrDefaultAsync(dr => dr.DroneId == droneId);

        if (d == null)
        {
            return NotFound(new { error = "Drone not found" });
        }

        double lat = d.Coordinate.Latitude;
        double lon = d.Coordinate.Longitude;

        var weather = await _weatherService.GetWeatherAsync(lat, lon);
        bool isGrounded = false;
        string reason = "";

        if (weather != null)
        {
            if (weather.Wind?.Speed >= 12.5)
            {
                isGrounded = true;
                reason += $"Silny wiatr ({weather.Wind.Speed} m/s). ";
            }

            if (weather.Weather?.Any(w => w.Main == "Rain" || w.Main == "Thunderstorm") == true)
            {
                isGrounded = true;
                reason += $"Zła pogoda: {weather.Weather[0].Main}. ";
            }

            if (weather.Rain?.OneHour > 0)
            {
                isGrounded = true;
                reason += $"Opady: {weather.Rain.OneHour} mm. ";
            }
        }

        string status = d.Status.ToString();

        var popupContent = $@"
        <strong>{d.CallSign}</strong><br>
        Status: {d.Status}<br>
        Model: {d.Model?.Name ?? "N/A"}<br>
        Range: {d.Range} m<br>
        Cloud: {d.DroneCloud?.Name ?? "Unassigned"}<br>";

        if (isGrounded)
        {
            popupContent += $"<span style='color:red;'><strong>UZIEMIONY:</strong> {reason}</span>";
        }

        var result = new
        {
            lat,
            lng = lon,
            isGrounded,
            droneId,
            status,
            popup = popupContent
        };

        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetDistrictData()
    {
        var districts = await _context.Districts
            .Include(d => d.BoundingPoints)
            .ToListAsync();

        var result = districts.Select(d => new
        {
            name = d.Name,
            boundaries = d.BoundingPoints
                .OrderBy(p => p.Id)
                .Select(p => new[] { p.Latitude, p.Longitude })
                .ToList()
        });

        return Json(result);
    }

    public async Task<IActionResult> Move(int droneId, double latitude, double longitude)
    {
        var drone = await _context.Drones
            .Include(d => d.Model)
            .Include(d => d.DroneCloud)
                .ThenInclude(dc => dc.District)
                    .ThenInclude(dist => dist.BoundingPoints)
            .FirstOrDefaultAsync(d => d.DroneId == droneId);

        if (drone == null)
            return NotFound(new { success = false, message = "Drone not found" });

        if (drone.DroneCloud?.District == null)
            return BadRequest(new { success = false, message = "Drone is not assigned to any district" });

        bool droneIsInFlight = _context.Flights.Any(f => f.DroneId == droneId && f.ArrivDate == null);
        if (droneIsInFlight)
            return BadRequest(new { success = false, message = "Drone is already in flight" });

        var districtBoundary = drone.DroneCloud.District.BoundingPoints.ToList();
        var currentPosition = drone.Coordinate;
        var targetPosition = new Coordinate { Latitude = latitude, Longitude = longitude };

        var (adjustedCoordinate, wasClipped, errorMessage) = _droneService.AdjustCoordinateToDistrictBoundary(
            districtBoundary,
            currentPosition,
            targetPosition);

        if (adjustedCoordinate == null)
        {
            return BadRequest(new { success = false, message = errorMessage });
        }

        var flight = new Flight
        {
            DroneId = droneId,
            DeliveryCoordinates = adjustedCoordinate
        };

        _context.Flights.Add(flight);
        await _context.SaveChangesAsync();

        _ = _droneService.MoveDroneAsync(droneId, adjustedCoordinate.Latitude, adjustedCoordinate.Longitude);

        return Ok(new
        {
            success = true,
            message = wasClipped ? "Cel poza district, lot do granicy." : "Drone move sent."
        });
    }




    [HttpGet]
    public async Task<IActionResult> GetFlightData(int droneId)
    {
        var flight = await _context.Flights
            .Include(f => f.Drone)
            .Where(f => f.DroneId == droneId && f.ArrivDate == null)
            .FirstOrDefaultAsync();

        if (flight == null)
        {
            return NotFound(new { message = "No active flight found for this drone." });
        }

        var result = new
        {
            flight.FlightId,
            flight.DepDate,
            flight.ArrivDate,
            flight.DroneId,
            flight.Steps,
            Coordinates = new
            {
                flight.DeliveryCoordinates.Latitude,
                flight.DeliveryCoordinates.Longitude
            }
        };

        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> Stop(int droneId)
    {
        var result = await _droneService.StopDrone(droneId);
        if (!result)
            return BadRequest("Drone was not moving or not found.");

        return Ok();
    }

    public async Task<IActionResult> GetFlightLog(int droneId)
    {
        var flights = await _context.Flights
            .Include(f => f.Drone)
            .Where(f => f.DroneId == droneId)
            .ToListAsync(); 

        if (flights == null || !flights.Any())
        {
            return NotFound(new { message = "No flights found for this drone." });
        }

        var result = flights.Select(f => new
        {
            f.FlightId,
            f.DepDate,
            f.ArrivDate,
            f.DroneId,
            f.Steps,
            Coordinates = new
            {
                f.DeliveryCoordinates.Latitude,
                f.DeliveryCoordinates.Longitude
            }
        }).ToList();

        return Json(result);
    }

}


