using Domain;
using Shared;

namespace Service.Login
{
    public interface ILoginService
    {
        Task<string> AuthorizeLogin(string email, string password);
        public Task<LogpunchUserDto> ValidateToken(string token);
    }
}
