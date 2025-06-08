using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Projekt_NET.Models;
using Projekt_NET.Models.System;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Projekt_NET.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DroneDbContext _context;
        private readonly AuthService _authService;
        public HomeController(ILogger<HomeController> logger, DroneDbContext context, AuthService authService)
        {
            _logger = logger;
            _logger = logger;
            _context = context;
            _authService = authService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult CRUD()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Packages()
        {
            return RedirectToAction("Index", "Packages");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Drones()
        {
            return RedirectToAction("Index", "Drones");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Deliveries()
        {
            return RedirectToAction("Index", "Deliveries");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Districts()
        {
            return RedirectToAction("Index", "Districts");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult DroneClouds()
        {
            return RedirectToAction("Index", "DroneClouds");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Operators()
        {
            return RedirectToAction("Index", "Operators");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Models()
        {
            return RedirectToAction("Index", "Models");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Clients()
        {
            return RedirectToAction("Index", "Clients");
        }
        public IActionResult Map()
        {
            return RedirectToAction("Index", "Map");
        }
        public IActionResult MapWeather()
        {
            return RedirectToAction("Weather", "Map");
        }

        [Authorize]
        public async Task<IActionResult> MyPackages()
        {
            var clientIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (clientIdClaim == null)
                return Unauthorized();

            int clientId = int.Parse(clientIdClaim.Value);

            var packages = await _context.Packages
                .Include(p => p.Drone)
                .Where(p => p.ClientId == clientId)
                .ToListAsync();

            return View(packages);
        }



        [Route("Account/[action]")]
        [HttpGet]
        public async Task<IActionResult> Login()
        {
                return View();
        }

        [Route("Account/[action]")]
        [HttpPost]
        public async Task<IActionResult> Login(string login, string password)
        {
            if (ModelState.IsValid)
            {
                if (_authService.Validate(login, password) != null)
                {
                    var claims = _authService.GetClaims(_authService.GetUser(login));
                    var principal = new ClaimsPrincipal(claims);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Alert = "Niepoprawny login lub has³o.";
                    return View();
                }
            }
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Name,Surname,PhoneNumber,Login,Password")] Client client)
        {
            if (ModelState.IsValid)
            {
                if (_context.Clients.Any(c => c.Login == client.Login))
                {
                    ModelState.AddModelError("Login", "Login ju¿ istnieje.");
                    return View(client);
                }

                var passwordHasher = new PasswordHasher<Client>();
                client.Password = passwordHasher.HashPassword(client, client.Password);
                client.Role = "User";

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                var claims = _authService.GetClaims(client);
                var principal = new ClaimsPrincipal(claims);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }

            return View(client);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
