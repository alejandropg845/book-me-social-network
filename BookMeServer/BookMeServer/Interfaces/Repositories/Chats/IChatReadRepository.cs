using BookMeServer.Models;
using BookMeServer.Responses.ChatResponses;
using Microsoft.Data.SqlClient;

namespace BookMeServer.Interfaces.Repositories.Chats
{
    public interface IChatReadRepository
    {
        Task<IEnumerable<Chat>> GetUserChats(SqlConnection connection, string currentUserId, string keyword);
        Task<IEnumerable<ChatMessage>> GetChatMessagesAsync(SqlConnection connection, string chatId, int number);
        Task<SendMessageResponse> ValidateSendMessageAsync(SqlConnection connection, string userId, string chatId, string otherUserId);

    }
}
