using System;
using System.Threading.Tasks;

namespace PlayServices.Services.Interfaces
{
    public interface IGameDataService
    {
        Task<uint?> GetCurrentIndex(Guid userId, string gameId);
        string GetDataFetchUrl(Guid userId, string gameId, uint index);
        Task<string> GetNextDataCreateUrl(Guid userId, string gameId);
    }
}