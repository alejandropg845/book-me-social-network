using BookMeServer.DTOs.Chat;
using BookMeServer.Models;
using BookMeServer.Responses.ChatResponses;

namespace BookMeServer.Interfaces.Services
{
    public interface IChatService
    {
        Task<IEnumerable<Chat>> GetUserChats(string currentUserId, string keyword);
        Task<IEnumerable<ChatMessage>> GetChatMessagesAsync(string chatId, int number);
        Task<SendMessageResponse> SendMessageAsync(SendMessageDto dto, string userId);
        Task SendMarkedAsReadMessagesToOtherUser(GetChatMessagesDto dto, string userId);
        Task<Chat> MarkAllUserChatMessagesAsReadAsync(string chatId, string userId);
    }
}
