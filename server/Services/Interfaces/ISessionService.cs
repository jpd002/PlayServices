using System;

namespace PlayServices.Services.Interfaces
{
    public interface ISessionService
    {
        string CreateSession(Guid userId);
    }
}
