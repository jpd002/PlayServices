using Microsoft.IdentityModel.Tokens;
using PlayServices.Services.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;

namespace PlayServices.Services
{
    public class SessionService : ISessionService
    {
        public string CreateSession(Guid userId)
        {
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable(ConfigKeys.g_env_accessTokenKey));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, userId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    };
}
