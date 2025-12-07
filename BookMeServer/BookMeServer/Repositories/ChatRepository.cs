using BookMeServer.DTOs.Chat;
using BookMeServer.Hubs;
using BookMeServer.Interfaces.Repositories.Chats;
using BookMeServer.Models;
using BookMeServer.Queries;
using BookMeServer.Responses.ChatResponses;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace BookMeServer.Repositories
{
    public class ChatRepository : IChatWriteRepository, IChatReadRepository
    {
        public async Task<IEnumerable<Chat>> GetUserChats(SqlConnection connection, string currentUserId, string keyword)
        {
            var chats = await connection.QueryAsync<Chat>(ChatQueries.GetChats,
                new { currentUserId, keyword });

            return chats;

        }

        public async Task<ChatMessage> AddMessageMarkedAsReadIfChatIsOpenAsync(SqlConnection connection, string otherUserId, string chatId, string message, string userId, string? userConnectionForChatOpen)
        {
            string queryInsertChatMessage;

            if (userConnectionForChatOpen is not null)
                queryInsertChatMessage = ChatQueries.MarkedAsReadMessage;
            else
                queryInsertChatMessage = ChatQueries.NotMarkedAsReadMessage;
                    
            /* Insertar y obtener el mensaje enviado */
            var sentMessage = await connection.QuerySingleAsync<ChatMessage>(
                queryInsertChatMessage,
                new { chatId, message, userId }
            );

            return sentMessage;
        }

        public async Task<MessagePopup> SendMessageNotification(SqlConnection connection, string userId, string message)
        {
            var r = await connection.QuerySingleAsync<(string ImageUrl, string Username)>
            (
                "SELECT ImageUrl, Username FROM Users WHERE Id = @userId;",
                new { userId }
            );

            MessagePopup newNotificationMessage = new()
            {
                ImageUrl = r.ImageUrl,
                Username = r.Username,
                Message = message,
                NotiType = "M",
                PostId = 0,
                UserId = 0
            };

            return newNotificationMessage;
        }

        public async Task<Chat> UpdateCurrentUserChatAsync(SqlConnection connection, string userId, string chatId, string otherUserId)
        {
            /* Actualizar y obtener vista previa del chat actualizada */
            var updatedCurrentUserChat = await connection.QuerySingleAsync<Chat>
            (
                ChatQueries.UpdatedCurrentUserChat,
                new { userId, chatId, otherUserId }
            );

            return updatedCurrentUserChat;
        }

        public async Task<Chat> UpdateOtherUserChatAsync(SqlConnection connection, string userId, string chatId, string otherUserId)
        {
            /* Actualizar y obtener vista previa del chat actualizada */
            var updatedCurrentUserChat = await connection.QuerySingleAsync<Chat>
            (
                ChatQueries.UpdatedOtherUserChat,
                new { userId, chatId, otherUserId }
            );

            return updatedCurrentUserChat;
        }
        public async Task<SendMessageResponse> ValidateSendMessageAsync(SqlConnection connection, string userId, string chatId, string otherUserId)
        {
            var (
                SenderExists,
                ReceiverExists,
                ChatExists,
                IsBlockedByUser,
                IsBlockedByOtherUser
            ) = await connection
            .QuerySingleAsync<(
            bool SenderExists,
            bool ReceiverExists,
            bool ChatExists,
            bool IsBlockedByUser,
            bool IsBlockedByOtherUser
            )>
            (
                ChatQueries.SendMessageValidations,
                new
                {
                    userId,
                    chatId,
                    otherUserId
                }
            );

            return new SendMessageResponse
            {
                SenderExists = SenderExists,
                ReceiverExists = ReceiverExists,
                ChatExists = ChatExists,
                IsBlockedByUser = IsBlockedByUser,
                IsBlockedByOtherUser = IsBlockedByOtherUser
            };
        }

        public async Task<IEnumerable<ChatMessage>> GetChatMessagesAsync(SqlConnection connection, string chatId,
            int number)
        {

            var messages = await connection.QueryAsync<ChatMessage>(
                ChatQueries.GetChatMessages,
                new { chatId, number }
            );

            return messages.Reverse();

        }
        public async Task OnlyMarkAsReadUserChatMessagesAsync(SqlConnection connection, string chatId, string userId)
        => await connection.ExecuteAsync(ChatQueries.MarkChatMessagesAsRead, new { chatId, userId });
        
        public async Task<Chat> MarkAllUserChatMessagesAsReadAsync(SqlConnection connection, IDbTransaction transaction, string chatId, string userId)
        {
            await connection.ExecuteAsync(
                ChatQueries.MarkChatMessagesAsRead,
                new { chatId, userId },
                transaction
            );

            Chat updatedChat = 
            await connection.QuerySingleAsync<Chat>(
                ChatQueries.GetUpdatedChat,
                new { chatId, userId },
                transaction
            );

            return updatedChat;
        }

    }
}

