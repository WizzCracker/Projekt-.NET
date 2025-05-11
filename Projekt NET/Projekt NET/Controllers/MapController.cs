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
                popup = popupContent
            });
        }

        return Json(new { markers = result });
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
        _ = _droneService.MoveDroneAsync(droneId, latitude, longitude);

        var flight = new Flight
        {
            DroneId = droneId,
            DeliveryCoordinates = new Coordinate
            {
                Latitude = latitude,
                Longitude = longitude
            }
        };

        _context.Flights.Add(flight);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

}


