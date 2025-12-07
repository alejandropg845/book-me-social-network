namespace BookMeServer.Interfaces.Services
{
    public interface IChatsConnectionsService
    {
        void SaveUserConnection(string userId, string connectionId);
        string? GetUserConnectionId(string userId);
        void RemoveUserConnection(string userId);
        void RemoveUserIdFromChatOpen(int userId, string chatId);
        void RemoveFromChatOpenWithNoChatId(string userId);
        void SaveUserIdChatOpen(int userId, string chatId, string connectionId);
        string GetUserConnectionForChatOpen(int userId, string chatId);
    }
}
