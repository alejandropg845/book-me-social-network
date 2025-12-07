using BookMeServer.Models;

namespace BookMeServer.Interfaces.Services
{
    public interface ITokenService
    {
        string CreateToken(int userId, string roleName);
    }
}
