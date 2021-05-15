using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using PlayServices.DataModel;

namespace PlayServices.Services.Interfaces
{
    public interface IGameDataService
    {
        Task<IEnumerable<GameDataInfo>> GetAvailableData(Guid userId);

        Task<GameDataInfo> GetDataInfo(Guid userId, string gameId);
        string GetDataFetchUrl(Guid userId, string gameId, uint index);
        Task<string> GetNextDataCreateUrl(Guid userId, string gameId);
    }
}