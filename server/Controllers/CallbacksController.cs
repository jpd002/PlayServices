using Microsoft.AspNetCore.Mvc;
using PlayServices.DataModel;
using System;
using System.Threading.Tasks;

namespace PlayServices.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CallbacksController : ControllerBase
    {
        PatreonApi _patreon = new PatreonApi();

        UserService _userService = new DataModel.UserService();
        SessionService _sessionService = new DataModel.SessionService();
        string _patreonClientId = Environment.GetEnvironmentVariable(ConfigKeys.g_env_ps_patreon_client_id);
        string _patreonClientSecret = Environment.GetEnvironmentVariable(ConfigKeys.g_env_ps_patreon_client_secret);
        string _patreonRedirectUri = Environment.GetEnvironmentVariable(ConfigKeys.g_env_ps_patreon_redirect_uri);

        [HttpGet("patreon")]
        public async Task<ActionResult> HandleRedirect([FromQuery] string code, [FromQuery] string state)
        {
            var tokenInfo = await _patreon.GetTokenAuthorizationCode(code, _patreonClientId, _patreonClientSecret, _patreonRedirectUri);
            _patreon.AccessToken = tokenInfo.AccessToken;
            var identityInfo = await _patreon.GetIdentity();
            var user = await _userService.GetUserFromPatreonId(identityInfo.Data.Id);
            if(user == null)
            {
                user = new User();
                user.Id = Guid.NewGuid();
                user.PatreonId = identityInfo.Data.Id;
                await _userService.SaveUser(user);
            }
            var sessionToken = _sessionService.CreateSession(user.Id);
            return Redirect(string.Format("playservices://loginsucceeded?token={0}", sessionToken));
        }
    }
}
