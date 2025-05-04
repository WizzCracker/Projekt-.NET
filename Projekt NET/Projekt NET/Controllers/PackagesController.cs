using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projekt_NET.Models;
using Projekt_NET.Models.System;
using Microsoft.AspNetCore.Authorization;

namespace Projekt_NET.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Paczki")]
    public class PackagesController : Controller
    {
        private readonly DroneDbContext _context;

        public PackagesController(DroneDbContext context)
        {
            _context = context;
        }

        // GET: Packages
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var droneDbContext = _context.Packages.Include(p => p.Client).Include(p => p.Drone);
            return View(await droneDbContext.ToListAsync());
        }   

        // GET: Packages/Details/5
        [Route("Szczegoly")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var package = await _context.Packages
                .Include(p => p.Client)
                .Include(p => p.Drone)
                .FirstOrDefaultAsync(m => m.PackageId == id);
            if (package == null)
            {
                return NotFound();
            }

            return View(package);
        }

        // GET: Packages/Create
        [Route("Dodaj")]
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "Name");
            ViewData["DroneId"] = new SelectList(_context.Drones, "DroneId", "CallSign");

            return View();
        }


        // POST: Packages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("Dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PackageId,ClientId,DroneId,Weight,TargetAddress")] Package package)
        {
            if (ModelState.IsValid)
            {
                var drone = await _context.Drones
                    .Include(d => d.Model)
                    .FirstOrDefaultAsync(d => d.DroneId == package.DroneId);

                if (drone == null || drone.Model == null)
                {
                    ModelState.AddModelError("", "Invalid drone or drone model.");
                    ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "Name", package.ClientId);
                    ViewData["DroneId"] = new SelectList(_context.Drones, "DroneId", "CallSign", package.DroneId);
                    return View(package);
                }

                if (package.Weight > drone.Model.MaxCapacity)
                {
                    ModelState.AddModelError("Weight", $"The package weight exceeds the drone's maximum carry capacity of {drone.Model.MaxCapacity}.");
                    ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "Name", package.ClientId);
                    ViewData["DroneId"] = new SelectList(_context.Drones, "DroneId", "CallSign", package.DroneId);
                    return View(package);
                }

                _context.Add(package);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "Name", package.ClientId);
            ViewData["DroneId"] = new SelectList(_context.Drones, "DroneId", "CallSign", package.DroneId);
            return View(package);
        }

        // GET: Packages/Edit/5
        [Route("Edycja/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var package = await _context.Packages.FindAsync(id);
            if (package == null)
            {
                return NotFound();
            }
            ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "Name", package.ClientId);
            ViewData["DroneId"] = new SelectList(_context.Drones, "DroneId", "CallSign", package.DroneId);
            return View(package);
        }

        // POST: Packages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("Edycja/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PackageId,ClientId,DroneId,Weight,TargetAddress")] Package package)
        {
            if (id != package.PackageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var drone = await _context.Drones
                    .Include(d => d.Model)
                    .FirstOrDefaultAsync(d => d.DroneId == package.DroneId);

                if (drone == null || drone.Model == null)
                {
                    ModelState.AddModelError("", "Invalid drone or drone model.");
                    ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "Name", package.ClientId);
                    ViewData["DroneId"] = new SelectList(_context.Drones, "DroneId", "CallSign", package.DroneId);
                    return View(package);
                }

                if (package.Weight > drone.Model.MaxCapacity)
                {
                    ModelState.AddModelError("Weight", $"The package weight exceeds the drone's maximum carry capacity of {drone.Model.MaxCapacity}.");
                    ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "Name", package.ClientId);
                    ViewData["DroneId"] = new SelectList(_context.Drones, "DroneId", "CallSign", package.DroneId);
                    return View(package);
                }
                try
                {
                    _context.Update(package);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PackageExists(package.PackageId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Packages");
            }
            ViewData["ClientId"] = new SelectList(_context.Clients, "ClientId", "Name", package.ClientId);
            ViewData["DroneId"] = new SelectList(_context.Drones, "DroneId", "CallSign", package.DroneId);
            return View(package);
        }

        // GET: Packages/Delete/5
        [Route("Usun/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var package = await _context.Packages
                .Include(p => p.Client)
                .Include(p => p.Drone)
                .FirstOrDefaultAsync(m => m.PackageId == id);
            if (package == null)
            {
                return NotFound();
            }

            return View(package);
        }

        // POST: Packages/Delete/5
        [HttpPost("Usun/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var package = await _context.Packages.FindAsync(id);
            if (package != null)
            {
                _context.Packages.Remove(package);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PackageExists(int id)
        {
            return _context.Packages.Any(e => e.PackageId == id);
        }
    }
}
