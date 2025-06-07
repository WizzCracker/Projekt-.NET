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
using System.Security.Claims;
using Projekt_NET.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace Projekt_NET.Controllers
{
    [Authorize(Roles = "Admin, User")]
    [Route("Paczki")]
    public class PackagesController : Controller
    {
        private readonly DroneDbContext _context;
        private readonly DroneService _droneService;
        private readonly GoogleGeocodingService _geoService;

        public PackagesController(DroneDbContext context, DroneService droneService, GoogleGeocodingService geoService)
        {
            _context = context;
            _droneService = droneService;
            _geoService = geoService;
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
            return View();
        }


        // POST: Packages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Route("Dodaj")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Weight,PickupAddress,TargetAddress")] Package package, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
                return View(package);

            var clientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (clientIdClaim == null)
                return Unauthorized();

            int clientId = int.Parse(clientIdClaim.Value);
            package.ClientId = clientId;

            var pickupCoords = await _droneService.GeocodeAddressAsync(package.PickupAddress);
            if (pickupCoords == null)
            {
                ModelState.AddModelError("PickupAddress", "Nie udało się zgeokodować adresu odbioru.");
                return View(package);
            }

            var deliveryCoords = await _droneService.GeocodeAddressAsync(package.TargetAddress);
            if (deliveryCoords == null)
            {
                ModelState.AddModelError("TargetAddress", "Nie udało się zgeokodować adresu dostawy.");
                return View(package);
            }

            double distance = GeoFunctions.HaversineDistance(pickupCoords.Value.lat, pickupCoords.Value.lng, deliveryCoords.Value.lat, deliveryCoords.Value.lng);
            double price = 20 + (package.Weight ?? 0) * 2 + distance;
            package.Price = price;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                package.ImagePath = "/uploads/" + uniqueFileName;
            }

            Drone drone = null;
            if (package.DroneId == null)
            {
                var freeDrones = _context.Drones
                    .Include(d => d.Coordinate)
                    .Include(d => d.Model)
                    .Where(d => d.Status == DStatus.Active && d.Coordinate != null)
                    .ToList();

                if (!freeDrones.Any())
                {
                    ModelState.AddModelError(string.Empty, "Brak dostępnych dronów. Spróbuj ponownie później.");
                    return View(package);
                }

                drone = freeDrones
                    .OrderBy(d => GeoFunctions.HaversineDistance(
                        d.Coordinate.Latitude,
                        d.Coordinate.Longitude,
                        pickupCoords.Value.lat,
                        pickupCoords.Value.lng))
                    .First();

                package.DroneId = drone.DroneId;
            }

            if (package.Weight > drone.Model.MaxCapacity)
            {
                ModelState.AddModelError("Weight", $"Masa paczki przekracza udźwig drona: {drone.Model.MaxCapacity} kg.");
                return View(package);
            }


            _context.Packages.Add(package);
            await _context.SaveChangesAsync();


            _ = Task.Run(async () =>
            {
                await _droneService.HandleCrossDistrictFlightAsync(drone.DroneId, pickupCoords.Value.lat, pickupCoords.Value.lng);

                await _droneService.HandleCrossDistrictFlightAsync(drone.DroneId, deliveryCoords.Value.lat, deliveryCoords.Value.lng, package.PackageId);
            });


            return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id, [Bind("PackageId,ClientId,DroneId,Weight,TargetAddress,PickupAddress")] Package package, IFormFile? ImageFile)
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

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(package.ImagePath))
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", package.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    package.ImagePath = "/uploads/" + uniqueFileName;
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

            if (!string.IsNullOrEmpty(package.ImagePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", package.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PackageExists(int id)
        {
            return _context.Packages.Any(e => e.PackageId == id);
        }

        [Authorize]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var package = await _context.Packages
                .Include(p => p.Client)
                .Include(p => p.Drone)
                .FirstOrDefaultAsync(p => p.PackageId == id);

            if (package == null)
                return NotFound();

            var delivery = await _context.Deliveries
                .Include(d => d.FlightPath)
                    .ThenInclude(fp => fp.FlightList)
                        .ThenInclude(f => f.Drone)
                .FirstOrDefaultAsync(d => d.PackageId == package.PackageId);

            var logs = new List<DeliveryLog>();
            if (delivery != null)
            {
                logs = await _context.DeliveryLogs
                    .Where(l => l.DeliveryId == delivery.DeliveryId)
                    .ToListAsync();
            }

            var pdf = GeneratePackagePdf(package, delivery, logs);


            var stream = new MemoryStream();
            pdf.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", $"Paczka_{package.PackageId}.pdf");
        }

        private Document GeneratePackagePdf(Package package, Delivery? delivery, List<DeliveryLog> logs)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(14));

                    page.Header()
                        .Text($"Szczegóły paczki #{package.PackageId}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Text($"Nadawca: {package.Client?.Name} {package.Client?.Surname}");
                        col.Item().Text($"Adres odbioru: {package.PickupAddress}");
                        col.Item().Text($"Adres dostawy: {package.TargetAddress}");
                        col.Item().Text($"Waga: {package.Weight ?? 0} kg");
                        col.Item().Text($"Dron początkowy: {(package.Drone != null ? package.Drone.CallSign : "Nieprzypisany")}");
                        col.Item().Text("");

                        if (delivery?.FlightPath?.FlightList != null && delivery.FlightPath.FlightList.Any())
                        {
                            col.Item().Text("Ścieżka lotu z paczką:").Bold().FontSize(16);

                            var flights = delivery.FlightPath.FlightList
                                .OrderBy(f => f.DepDate)
                                .Where((f, index) => index % 2 == 0)
                                .ToList();

                            foreach (var flight in flights)
                            {
                                var dep = flight.DepDate.ToString("yyyy-MM-dd");
                                var arr = flight.ArrivDate?.ToString("yyyy-MM-dd") ?? "-";
                                var coords = $"{flight.DeliveryCoordinates.Latitude}, {flight.DeliveryCoordinates.Longitude}";
                                var drone = flight.Drone?.CallSign ?? "Brak";

                                col.Item().Text($"Data odlotu: {dep}");
                                col.Item().Text($"Data przylotu: {arr}");
                                col.Item().Text($"Cel: {coords}");
                                col.Item().Text($"Dron: {drone}");
                                col.Item().Text("");
                            }


                            col.Item().Text("");
                        }

                        if (logs.Any())
                        {
                            col.Item().Text("Dziennik dostawy:").Bold().FontSize(16);
                            foreach (var log in logs.OrderBy(l => l.LogDate))
                            {
                                col.Item().Text($" - {log.LogDate:yyyy-MM-dd HH:mm}: {log.Remarks}");
                            }
                        }
                    });
                });
            });
        }



        [HttpGet]
        [Route("GetDistance")]
        public async Task<IActionResult> GetDistance(string origin, string destination)
        {
            if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
                return BadRequest("Invalid addresses.");

            try
            {
                var originDecoded = await _droneService.GeocodeAddressAsync(origin);
                var destinationDecoded = await _droneService.GeocodeAddressAsync(destination);
                var distance = GeoFunctions.HaversineDistance(
                        originDecoded.Value.lat,
                        originDecoded.Value.lng,
                        destinationDecoded.Value.lat,
                        destinationDecoded.Value.lng);
                return Json(distance);
            }

            catch (Exception)
            {
                return Json(0);
            }
        }

    }
}
