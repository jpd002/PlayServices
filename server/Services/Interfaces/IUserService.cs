using PlayServices.DataModel;
using System;
using System.Threading.Tasks;

namespace PlayServices.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUser(Guid userId);
        Task<User> GetUserFromPatreonId(uint patreonId);

        Task SaveUser(User user);
    }
}
