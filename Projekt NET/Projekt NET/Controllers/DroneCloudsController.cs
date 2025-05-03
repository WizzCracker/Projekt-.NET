using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projekt_NET.Models;
using Projekt_NET.Models.System;

namespace Projekt_NET.Controllers
{
    public class DroneCloudsController : Controller
    {
        private readonly DroneDbContext _context;

        public DroneCloudsController(DroneDbContext context)
        {
            _context = context;
        }

        // GET: DroneClouds
        public async Task<IActionResult> Index()
        {
            var droneDbContext = _context.DroneClouds.Include(d => d.District);
            return View(await droneDbContext.ToListAsync());
        }

        // GET: DroneClouds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var droneCloud = await _context.DroneClouds
                .Include(d => d.District)
                .FirstOrDefaultAsync(m => m.DroneCloudId == id);
            if (droneCloud == null)
            {
                return NotFound();
            }

            return View(droneCloud);
        }

        // GET: DroneClouds/Create
        public IActionResult Create()
        {
            ViewData["DistrictId"] = new SelectList(_context.Districts, "DistrictId", "Name");
            return View();
        }

        // POST: DroneClouds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DroneCloudId,DistrictId")] DroneCloud droneCloud)
        {
            droneCloud.Drones = new List<Drone>();
            if (ModelState.IsValid)
            {
                _context.Add(droneCloud);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DistrictId"] = new SelectList(_context.Districts, "DistrictId", "Name", droneCloud.DistrictId);
            return View(droneCloud);
        }

        // GET: DroneClouds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var droneCloud = await _context.DroneClouds.FindAsync(id);
            if (droneCloud == null)
            {
                return NotFound();
            }
            ViewData["DistrictId"] = new SelectList(_context.Districts, "DistrictId", "Name", droneCloud.DistrictId);
            return View(droneCloud);
        }

        // POST: DroneClouds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DroneCloudId,DistrictId")] DroneCloud droneCloud)
        {
            if (id != droneCloud.DroneCloudId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(droneCloud);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DroneCloudExists(droneCloud.DroneCloudId))
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
            ViewData["DistrictId"] = new SelectList(_context.Districts, "DistrictId", "Name", droneCloud.DistrictId);
            return View(droneCloud);
        }

        // GET: DroneClouds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var droneCloud = await _context.DroneClouds
                .Include(d => d.District)
                .FirstOrDefaultAsync(m => m.DroneCloudId == id);
            if (droneCloud == null)
            {
                return NotFound();
            }

            return View(droneCloud);
        }

        // POST: DroneClouds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var droneCloud = await _context.DroneClouds.FindAsync(id);
            if (droneCloud != null)
            {
                _context.DroneClouds.Remove(droneCloud);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DroneCloudExists(int id)
        {
            return _context.DroneClouds.Any(e => e.DroneCloudId == id);
        }
    }
}
