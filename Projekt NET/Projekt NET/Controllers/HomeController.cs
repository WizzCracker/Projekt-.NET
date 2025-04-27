using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Projekt_NET.Models;
using Projekt_NET.Models.System;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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
        public IActionResult Packages()
        {
            return RedirectToAction("Index", "Packages");
        }

        public IActionResult Login()
        {
            return View();
        }

        [Route("Account/[action]")]
        public async Task<IActionResult> Login(string login, string password)
        {
            if (Request.Method == "GET")
                return View();

            if (_authService.Validate(login, password) != null)
            {
                var claims = _authService.GetClaims(_authService.GetUser(login));
                var principal = new ClaimsPrincipal(claims);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewBag.Alert = "Niepoprawny login lub has³o.";
                return View();
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
