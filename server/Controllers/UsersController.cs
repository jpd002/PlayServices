using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using PlayServices.DataModel;

namespace PlayServices.Server.Controllers
{
    class CreateResponse
    {
        public Guid Id { get; set; } = Guid.Empty;

        public string GuestToken { get; set; } = string.Empty;
    };

    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        UserService _userService = new DataModel.UserService();

        public static string GenerateRandomCryptographicKey(int keyLength)
        {
            var generator = RandomNumberGenerator.Create();
            byte[] randomBytes = new byte[keyLength];
            generator.GetNonZeroBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        [HttpPost]
        public async Task<ActionResult> Create()
        {
            //Create guest user
            var guestToken = GenerateRandomCryptographicKey(32);

            var user = new User();
            user.Id = Guid.NewGuid();
            user.GuestAuth = new User.GuestAuthMethod { Token = guestToken };
            await _userService.SaveUser(user);

            //Return response
            var response = new CreateResponse();
            response.Id = user.Id;
            response.GuestToken = guestToken;
            return Ok(response);
        }

        [HttpPost("{id}/sessions")]
        public async Task<ActionResult> CreateSession(Guid id, [FromForm] string authType, [FromForm] string authToken)
        {
            //Only guest login is supported
            if(authType != "guest")
            {
                return Unauthorized();
            }

            var user = await _userService.GetUser(id);
            if(user.GuestAuth == null)
            {
                return Unauthorized();
            }

            if(user.GuestAuth.Token != authToken)
            {
                return Unauthorized();
            }

            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("accessTokenKey"));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(tokenHandler.WriteToken(token));
        }

        [Authorize("CanAccessSelfInfo")]
        [HttpPost("{id}/links")]
        public async Task<ActionResult> CreateLink(Guid id)
        {
            return Ok();
        }
    }
}
