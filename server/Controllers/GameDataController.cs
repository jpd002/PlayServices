using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PlayServices.DataModel;
using PlayServices.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace PlayServices.Server.Controllers
{
    [ApiController]
    [Route("api/users/{userIdOrMe}/[controller]")]
    public class GameDataController : ControllerBase
    {
        struct GetResponse
        {
            public GameDataInfo DataInfo { get; set; }
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

        [HttpPost("{gameId}")]
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
        public async Task<ActionResult> GetAll(string userIdOrMe)
        {
            var userId = GetUserIdFromParam(userIdOrMe);
            var result = await _gameDataService.GetAvailableData(userId);
            return Ok(result);
        }

        [HttpGet("{gameId}")]
        [Authorize("CanAccessSelfInfo")]
        public async Task<ActionResult> Get(string userIdOrMe, string gameId)
        {
            var userId = GetUserIdFromParam(userIdOrMe);
            var dataInfo = await _gameDataService.GetDataInfo(userId, gameId);
            if(dataInfo == null)
            {
                return NotFound();
            }
            var fetchUrl = _gameDataService.GetDataFetchUrl(userId, gameId, dataInfo.CurrentIndex);
            var response = new GetResponse
            {
                DataInfo = dataInfo,
                Url = fetchUrl,
            };
            return Ok(response);
        }
    }
}
