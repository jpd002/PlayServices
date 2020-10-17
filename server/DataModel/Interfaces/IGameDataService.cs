using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlayServices.DataModel.Interfaces
{
    public interface IGameDataService
    {
        Task<uint?> GetCurrentIndex(string userId, string gameId);
        string GetDataFetchUrl(string userId, string gameId, uint index);
        Task<string> GetNextDataCreateUrl(string userId, string gameId);
    }
}