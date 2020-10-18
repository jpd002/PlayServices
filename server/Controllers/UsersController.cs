using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayServices.Services.Interfaces;

namespace PlayServices.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        IUserService _userService;
        ISessionService _sessionService;

        public UsersController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult> SelfInfo()
        {
            var userId = Guid.Parse(User.Identity.Name);
            return Ok(userId);
        }

        [HttpPost("{userId}/login")]
        public async Task<ActionResult> Login(Guid userId, [FromForm] string authToken)
        {
            if(authToken != Environment.GetEnvironmentVariable(ConfigKeys.g_env_accessTokenKey))
            {
                return Unauthorized();
            }
            var token = _sessionService.CreateSession(userId);
            return Ok(token);
        }
    }
}
