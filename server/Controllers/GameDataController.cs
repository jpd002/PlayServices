using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PlayServices.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace PlayServices.Server.Controllers
{
    [ApiController]
    [Route("api/users/{userIdOrMe}/gameData/{gameId}")]
    public class GameDataController : ControllerBase
    {
        struct GetResponse
        {
            public uint? CurrentIndex { get; set; }
            public string Url { get; set; }
        }
        
        struct PostResponse
        {
            public string Url { get; set; }
        }

        IGameDataService _gameDataService;

        public GameDataController(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }

        Guid GetUserIdFromParam(string userIdOrMe)
        {
            if(userIdOrMe == "me")
            {
                return Guid.Parse(User.Identity.Name);
            }
            else
            {
                return Guid.Parse(userIdOrMe);
            }
        }

        [HttpPost]
        [Authorize("CanAccessSelfInfo")]
        public async Task<ActionResult> Create(string userIdOrMe, string gameId)
        {
            var userId = GetUserIdFromParam(userIdOrMe);
            var createUrl = await _gameDataService.GetNextDataCreateUrl(userId, gameId);
            var response = new PostResponse
            {
                Url = createUrl,
            };
            return Ok(response);
        }

        [HttpGet]
        [Authorize("CanAccessSelfInfo")]
        public async Task<ActionResult> Get(string userIdOrMe, string gameId)
        {
            var userId = GetUserIdFromParam(userIdOrMe);
            var currentIndex = await _gameDataService.GetCurrentIndex(userId, gameId);
            var fetchUrl = currentIndex.HasValue ? _gameDataService.GetDataFetchUrl(userId, gameId, currentIndex.Value) : string.Empty;
            var response = new GetResponse
            {
                CurrentIndex = currentIndex,
                Url = fetchUrl,
            };
            return Ok(response);
        }
    }
}
