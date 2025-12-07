using BookMeServer.DTOs.Chat;
using BookMeServer.Models;
using BookMeServer.Responses.ChatResponses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace BookMeServer.Interfaces.Repositories.Chats
{
    public interface IChatWriteRepository
    {
        Task<Chat> MarkAllUserChatMessagesAsReadAsync(SqlConnection connection, IDbTransaction transaction, string chatId, string userId);
        Task OnlyMarkAsReadUserChatMessagesAsync(SqlConnection connection, string chatId, string userId);
        Task<ChatMessage> AddMessageMarkedAsReadIfChatIsOpenAsync(SqlConnection connection, string otherUserId, string chatId, string message, string userId, string? userConnectionForChatOpen);
        Task<MessagePopup> SendMessageNotification(SqlConnection connection, string userId, string message);
        Task<Chat> UpdateCurrentUserChatAsync(SqlConnection connection, string userId, string chatId, string otherUserId);
        Task<Chat> UpdateOtherUserChatAsync(SqlConnection connection, string userId, string chatId, string otherUserId);
    }
}
