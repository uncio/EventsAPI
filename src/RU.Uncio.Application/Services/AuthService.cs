using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RU.Uncio.Application.Interfaces;
using RU.Uncio.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RU.Uncio.Application.Services
{
    /// <summary>
    /// Token service
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IConfiguration configuration;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conf"></param>
        public AuthService(IConfiguration conf)
        {
            configuration = conf;
        }

        /// <summary>
        /// Create token for authenticated user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string CreateToken(User user)
        {
            var claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                ["role"] = user.Role.ToString(),
            };

            var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = configuration["Jwt:Issuer"],
                Audience = configuration["Jwt:Audience"],
                Claims = claims,
                Expires = DateTime.UtcNow.AddMinutes(Int32.TryParse(configuration["Jwt:ExpiryMinutes"], out int mins) ? mins : 30),
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = creds
            };

            var tokenString = new JsonWebTokenHandler().CreateToken(descriptor);

            return tokenString;
        }
    }
}
