using FIAPDesafioPleno.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FIAPDesafioPleno.Services
{
    public class AuthService : IAuthService
    {
        private readonly PasswordHasher<Aluno> _hasher = new();
        private readonly IConfiguration _config;

        public AuthService(IConfiguration config)
        {
            _config = config;
        }

        public string HashPassword(string password) =>
            _hasher.HashPassword(null, password);

        public bool VerifyPassword(string hashed, string provided) =>
            _hasher.VerifyHashedPassword(null, hashed, provided) != PasswordVerificationResult.Failed;

        public string GenerateJwtToken(Aluno aluno)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = jwtSection["Key"];
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var expiresMinutes = int.Parse(jwtSection["ExpiresMinutes"] ?? "60");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, aluno.Id.ToString()),
                new Claim(ClaimTypes.Name, aluno.Email ?? ""),
                new Claim(ClaimTypes.Role, "Administrator") // Somente admin que acessa
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddMinutes(expiresMinutes), signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
