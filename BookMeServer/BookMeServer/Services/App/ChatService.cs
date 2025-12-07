using Azure;
using BookMeServer.DTOs.Chat;
using BookMeServer.Hubs;
using BookMeServer.Interfaces.Repositories.Chats;
using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using BookMeServer.Responses.ChatResponses;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Transactions;

namespace BookMeServer.Services.App
{
    public class ChatService : IChatService
    {
        private readonly IChatWriteRepository _chatWrite;
        private readonly string _connectionString;
        private readonly IHubContext<UserChatsHub> _userChatsHub;
        private readonly IChatsConnectionsService _userChatsConnectionsService;
        private readonly INotificationsService _notificationsService;
        private readonly IHubContext<NotificationsHub> _notisHub;
        private readonly ILogger<ChatService> _logger;
        private readonly IChatReadRepository _chatRead;
        public ChatService(
            IChatWriteRepository repo,
            IConfiguration config,
            IHubContext<UserChatsHub> userChatsHub,
            IChatsConnectionsService chatsConnectionService,
            INotificationsService notificationsService,
            IHubContext<NotificationsHub> notisHub,
            ILogger<ChatService> logger,
            IChatReadRepository chatRead
        )
        {
            _chatRead = chatRead;
            _chatWrite = repo;
            _connectionString = config["ConnectionStrings:SqlServerConnection"]!;
            _notificationsService = notificationsService;
            _userChatsHub = userChatsHub;
            _logger = logger;
            _notisHub = notisHub;
            _userChatsConnectionsService = chatsConnectionService;
        }
        public async Task<IEnumerable<Chat>> GetUserChats(string currentUserId, string keyword)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            return await _chatRead.GetUserChats(connection, currentUserId, keyword);
        }
        public async Task<IEnumerable<ChatMessage>> GetChatMessagesAsync(string chatId, int number)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await _chatRead.GetChatMessagesAsync(connection, chatId, number);
        }
        public async Task<SendMessageResponse> SendMessageAsync(SendMessageDto dto, string userId)
        {

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var response = await _chatRead.ValidateSendMessageAsync(
                connection,
                userId,
                dto.ChatId,
                dto.OtherUserId
            );

            if (!response.SenderExists) return response;

            if (!response.ReceiverExists) return response;

            if (!response.ChatExists) return response;

            if (response.IsBlockedByUser) return response;

            if (response.IsBlockedByOtherUser) return response;

            /* Obtener si el usuario se encuentra con el chat abierto */
            string? otherUserChatOpenConnectionId = _userChatsConnectionsService.GetUserConnectionForChatOpen(int.Parse(dto.OtherUserId), dto.ChatId);



            /* Agregar message si leido o no dependiendo si está abierto el chat del otro usuario */
            var sentMessage = await _chatWrite.AddMessageMarkedAsReadIfChatIsOpenAsync(
                connection,
                dto.OtherUserId,
                dto.ChatId,
                dto.Message,
                userId,
                otherUserChatOpenConnectionId
            );

            /* Procesos de hub */
            await SendHubInfoToUsers(
                connection,
                sentMessage,
                userId,
                dto.ChatId,
                dto.OtherUserId
            );

            return response;

        }
        private async Task SendHubInfoToUsers(SqlConnection connection, ChatMessage sentMessage, string userId, string chatId, string otherUserId)
        {
            /* Obtener si el usuario se encuentra con el chat abierto */
            string? currentUserChatOpenConnectionId = _userChatsConnectionsService.GetUserConnectionForChatOpen(int.Parse(userId), chatId);
            string? otherUserChatOpenConnectionId = _userChatsConnectionsService.GetUserConnectionForChatOpen(int.Parse(otherUserId), chatId);

            /* Obtener si el usuario está conectado */
            string? currentUserConnectionId = _userChatsConnectionsService.GetUserConnectionId(userId);
            string? otherUserConnectionId = _userChatsConnectionsService.GetUserConnectionId(otherUserId);


            /* Enviar mensaje enviado al usuario actual */
            await _userChatsHub.Clients.Client(currentUserChatOpenConnectionId)
            .SendAsync("ReceivedMessage", sentMessage);

            /* Hacer update de la vista previa del chat para el otro usuario (no de la ventana de chat) */
            var updatedCurrentUserChat = await _chatWrite.UpdateCurrentUserChatAsync(
                connection,
                userId,
                chatId,
                otherUserId
            );

            /* Enviar vista previa del chat al usuario actual*/
            await _userChatsHub.Clients.Client(currentUserConnectionId!)
                .SendAsync("ReceivedUserChat", updatedCurrentUserChat);

            //Enviar mensaje al chat al otro usuario
            if (otherUserChatOpenConnectionId is not null)
                await _userChatsHub.Clients.Client(otherUserChatOpenConnectionId)
                .SendAsync("ReceivedMessage", sentMessage);


            string? otherUserNotiConnectionId
                = _notificationsService.GetUserConnection(otherUserId);

            /* El otro usuario se encuentra conectado y recibe notificaciones */
            if (otherUserNotiConnectionId is not null)
            {

                /* Crear notification temporal de nuevo mensaje */
                var newNotificationMessage = await _chatWrite.SendMessageNotification(
                    connection,
                    userId,
                    sentMessage.Message
                );

                /* Enviar notificación de nuevo mensaje al otro usuario */
                await _notisHub.Clients.Client(otherUserNotiConnectionId)
                    .SendAsync("ReceivedAnyNotification", newNotificationMessage);


            }

            /* El otro usuario se encuentra en la sección de chats */
            if (otherUserConnectionId is not null)
            {
                /* Hacer update de la vista previa del chat (no de la ventana de chat) */
                var updatedOtherUserChat = await _chatWrite.UpdateOtherUserChatAsync(
                    connection,
                    userId,
                    chatId,
                    otherUserId
                );

                //Enviar vista previa del chat actualizado al otro usuario
                await _userChatsHub.Clients.Client(otherUserConnectionId)
                    .SendAsync("ReceivedUserChat", updatedOtherUserChat);

            }
        }
        public async Task SendMarkedAsReadMessagesToOtherUser(GetChatMessagesDto dto, string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            string otherUserConnectionId = _userChatsConnectionsService
                    .GetUserConnectionForChatOpen(dto.OtherUserId, dto.ChatId);

            await _chatWrite.OnlyMarkAsReadUserChatMessagesAsync(connection, dto.ChatId, userId);

            if (otherUserConnectionId is not null && dto.ChatMessages is not null)
            {
                //Obtener los mensajes que son del otro usuario y no han sido vistos
                var otherUserMessages = dto.ChatMessages.Where
                (
                    m => m.IsMarkedAsRead == false 
                    && m.UserId == dto.OtherUserId
                )
                .ToList();


                // El otro usuario tiene el chat abierto y hay mensajes de este sin leer
                if (otherUserMessages.Count > 0)
                {
                    foreach (var message in otherUserMessages)
                    {
                        message.IsMarkedAsRead = true;
                    }

                    await _userChatsHub.Clients.Client(otherUserConnectionId)
                        .SendAsync("ReceivedUpdatedMessages", otherUserMessages);

                }
            }

        }
        public async Task<Chat> MarkAllUserChatMessagesAsReadAsync(string chatId, string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var transaction = await connection.BeginTransactionAsync();

            try
            {
                var updatedChat = await _chatWrite.MarkAllUserChatMessagesAsReadAsync(connection, transaction, chatId, userId);
                await transaction.CommitAsync();

                return updatedChat;
            } catch (Exception ex)
            {
                _logger.LogError(
                    "No se pudo completar transaction en método {methodName}\nExcepción: {Msg}\nStackTrace: {StackTrace}",
                    nameof(MarkAllUserChatMessagesAsReadAsync), ex.Message, ex.StackTrace
                );
                await transaction.RollbackAsync();
                throw;
            }

        }
    }
}
