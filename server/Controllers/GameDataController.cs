using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PlayServices.Services.Interfaces;
using System.Threading.Tasks;

namespace PlayServices.Server.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/gameData/{gameId}")]
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

        [HttpPost]
        [Authorize("CanAccessSelfInfo")]
        public async Task<ActionResult> Create(string userId, string gameId)
        {
            var createUrl = await _gameDataService.GetNextDataCreateUrl(userId, gameId);
            var response = new PostResponse
            {
                Url = createUrl,
            };
            return Ok(response);
        }

        [HttpGet]
        [Authorize("CanAccessSelfInfo")]
        public async Task<ActionResult> Get(string userId, string gameId)
        {
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
