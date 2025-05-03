using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt_NET.Models; 
using Projekt_NET.Models.System;   

public class MapController : Controller
{
    private readonly DroneDbContext _context;

    public MapController(DroneDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> GetDroneData()
    {
        var drones = await _context.Drones
            .Include(d => d.Model)
            .Include(d => d.DroneCloud)
            .ToListAsync();

        var result = drones.Select(d => new
        {
            lat = d.Coordinate.Latitude,
            lng = d.Coordinate.Longitude,
            popup = $@"
            <strong>{d.CallSign}</strong><br>
            Status: {d.Status}<br>
            Model: {d.Model?.Name ?? "N/A"}<br>
            Range: {d.Range} km<br>
            Cloud: {d.DroneCloud?.Name ?? "Unassigned"}"
        });

        return Json(new { markers = result });
    }
}


