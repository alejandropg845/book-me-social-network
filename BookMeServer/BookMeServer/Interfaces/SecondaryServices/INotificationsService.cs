using BookMeServer.Models;

namespace BookMeServer.Interfaces.Services
{
    public interface INotificationsService
    {
        void SaveUserConnection(string userId, string connectionId);
        void RemoveDisconnectedUser(string userId);
        string? GetUserConnection(string userId);
    }
}
