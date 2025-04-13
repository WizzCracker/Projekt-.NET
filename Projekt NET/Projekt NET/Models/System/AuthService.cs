using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Principal;
    
namespace Projekt_NET.Models.System
{
    public class AuthService
    {
        private readonly DroneDbContext _context;

        public AuthService(DroneDbContext context)
        {
            _context = context;
        }

        public ClaimsIdentity GetClaims(Client client)
        {
            var result = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            result.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
            result.AddClaim(new Claim(ClaimTypes.Name, "Admin Profile"));
            result.AddClaim(new Claim("Login", client.Login ?? string.Empty));
            return result;
        }

        public Client? Validate(string login, string password)
        {
            return _context.Clients.FirstOrDefault(c => c.Login == login && c.Password == password);
        }

        public Client GetUser(string login)
        {
            return _context.Clients.First(c => c.Login == login);
        }
    }

}
