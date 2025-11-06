using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpiryMinutes;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secret = configuration["JWT:SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey not configured in appsettings.json");
            _issuer = configuration["JWT:Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer not configured in appsettings.json");
            _audience = configuration["JWT:Audience"]
                ?? throw new InvalidOperationException("JWT Audience not configured in appsettings.json");
            _accessTokenExpiryMinutes = int.Parse(configuration["JWT:AccessTokenExpirationMinutes"] ?? "60");
        }

        public string GenerateAccessToken(User user)
        {
            try
            {
                // ✅ Validate user object
                if (user == null)
                    throw new ArgumentNullException(nameof(user), "User cannot be null");

                Console.WriteLine($"=== DEBUG GenerateAccessToken ===");
                Console.WriteLine($"User ID: {user.Id}");
                Console.WriteLine($"Email: {user.Email ?? "NULL"}");
                Console.WriteLine($"FullName: {user.FullName ?? "NULL"}");
                Console.WriteLine($"Role: {user.Role}");
                Console.WriteLine($"Status: {user.Status}");
                Console.WriteLine($"PhoneNumber: {user.PhoneNumber ?? "NULL"}");

                if (string.IsNullOrEmpty(user.Email))
                    throw new ArgumentException("User email is required", nameof(user));

                Console.WriteLine("Creating security key...");
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                Console.WriteLine("Creating claims...");
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim("Status", user.Status.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                // Add phone number if available
                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
                }

                Console.WriteLine("Creating JWT token...");
                var token = new JwtSecurityToken(
                    issuer: _issuer,
                    audience: _audience,
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
                    signingCredentials: credentials
                );

                Console.WriteLine("Writing token...");
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                Console.WriteLine("Token generated successfully!");

                return tokenString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GenerateAccessToken: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secret);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // Verify it's a JWT token
                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                // Token expired
                return null;
            }
            catch (Exception)
            {
                // Invalid token
                return null;
            }
        }

        public int? GetUserIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null)
                return null;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        public string? GetEmailFromToken(string token)
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst(ClaimTypes.Email)?.Value;
        }

        public bool IsTokenExpired(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                return jwtToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }
    }
}