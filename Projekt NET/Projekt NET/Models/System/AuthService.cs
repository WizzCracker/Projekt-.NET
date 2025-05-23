﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
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
            result.AddClaim(new Claim(ClaimTypes.NameIdentifier, client.ClientId.ToString()));
            result.AddClaim(new Claim(ClaimTypes.Role, client.Role ?? "User"));
            result.AddClaim(new Claim(ClaimTypes.Name, client.Login ?? "Unknown"));
            return result;
        }

        public Client? Validate(string login, string password)
        {
            var user = _context.Clients.FirstOrDefault(c => c.Login == login);
            if (user == null)
                return null;

            var hasher = new PasswordHasher<Client>();
            var result = hasher.VerifyHashedPassword(user, user.Password, password);

            return result == PasswordVerificationResult.Success ? user : null;
        }

        public Client GetUser(string login)
        {
            return _context.Clients.First(c => c.Login == login);
        }
    }

}
