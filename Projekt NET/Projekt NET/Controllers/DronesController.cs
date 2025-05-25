using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projekt_NET.Models;
using Projekt_NET.Models.System;
using Microsoft.AspNetCore.Authorization;
using Projekt_NET.Services;

namespace Projekt_NET.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DronesController : Controller
    {
        private readonly DroneDbContext _context;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly DroneService _droneService;

        public DronesController(DroneDbContext context, IServiceScopeFactory scopeFactory, DroneService droneService)
        {
            _context = context;
            _scopeFactory = scopeFactory;
            _droneService = droneService;
        }


        // GET: Drones
        public async Task<IActionResult> Index()
        {
            var droneDbContext = _context.Drones.Include(d => d.DroneCloud).Include(d => d.Model);
            return View(await droneDbContext.ToListAsync());
        }

        // GET: Drones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drone = await _context.Drones
                .Include(d => d.DroneCloud)
                .Include(d => d.Model)
                .FirstOrDefaultAsync(m => m.DroneId == id);
            if (drone == null)
            {
                return NotFound();
            }

            return View(drone);
        }

        // GET: Drones/Create
        public IActionResult Create()
        {
            ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(DStatus)));

            var droneClouds = _context.DroneClouds.ToList();
            ViewData["DroneCloudId"] = new SelectList(droneClouds, "DroneCloudId", "Name");


            ViewData["ModelId"] = new SelectList(_context.Models, "ModelId", "Name");
            return View();
        }


        // POST: Drones/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DroneId,CallSign,Status,Coordinate,ModelId,Range,DroneCloudId,AqDate")] Drone drone)
        {
            if (ModelState.IsValid)
            {
                _context.Add(drone);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(DStatus)));

            var droneClouds = _context.DroneClouds
                .Select(dc => new { DroneCloudId = (int?)dc.DroneCloudId, Name = dc.Name })
                .ToList();

            droneClouds.Insert(0, new { DroneCloudId = (int?)null, Name = "Unassigned" });

            ViewData["DroneCloudId"] = new SelectList(droneClouds, "DroneCloudId", "Name", drone.DroneCloudId);

            ViewData["ModelId"] = new SelectList(_context.Models, "ModelId", "Name", drone.ModelId);
            return View(drone);
        }


        // GET: Drones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drone = await _context.Drones.FindAsync(id);
            if (drone == null)
            {
                return NotFound();
            }
            ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(DStatus)));
            ViewData["DroneCloudId"] = new SelectList(_context.DroneClouds, "DroneCloudId", "Name", drone.DroneCloudId);
            ViewData["ModelId"] = new SelectList(_context.Models, "ModelId", "Name", drone.ModelId);
            return View(drone);
        }

        // POST: Drones/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DroneId,CallSign,Status,Coordinate,ModelId,Range,DroneCloudId,AqDate")] Drone drone)
        {
            if (id != drone.DroneId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(drone);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DroneExists(drone.DroneId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["StatusList"] = new SelectList(Enum.GetValues(typeof(DStatus)));
            ViewData["DroneCloudId"] = new SelectList(_context.DroneClouds, "DroneCloudId", "Name", drone.DroneCloudId);
            ViewData["ModelId"] = new SelectList(_context.Models, "ModelId", "Name", drone.ModelId);
            return View(drone);
        }

        // GET: Drones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drone = await _context.Drones
                .Include(d => d.DroneCloud)
                .Include(d => d.Model)
                .FirstOrDefaultAsync(m => m.DroneId == id);
            if (drone == null)
            {
                return NotFound();
            }

            return View(drone);
        }

        // POST: Drones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var drone = await _context.Drones.FindAsync(id);
            if (drone != null)
            {
                _context.Drones.Remove(drone);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DroneExists(int id)
        {
            return _context.Drones.Any(e => e.DroneId == id);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Move(int droneId, double latitude, double longitude)
        {


            bool droneIsInFlight = _context.Flights
            .Any(f => f.DroneId == droneId && f.ArrivDate == null);
            if (droneIsInFlight)
            {
                TempData["Error"] = "Drone is already in flight!";
                return RedirectToAction(nameof(Edit), new { id = droneId });
            }

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

            return RedirectToAction(nameof(Edit), new { id = droneId });
        }

    }
}
