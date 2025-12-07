using BookMeServer.DTOs.FollowRequest;
using BookMeServer.Hubs;
using BookMeServer.Interfaces.Repositories.Notifications;
using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using BookMeServer.Queries;
using BookMeServer.Responses.FollowRequestResponses;
using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;

namespace BookMeServer.Services.App
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationsWriteRepository _notisWrite;
        private readonly IHubContext<NotificationsHub> _notificationsHubContext;
        private readonly INotificationsReadRepository _notisRead;
        private readonly INotificationsService _userNotificationsService;
        private readonly string _stringConnection;
        public NotificationService(INotificationsWriteRepository repo, IHubContext<NotificationsHub> nhc, IConfiguration config, INotificationsService notificationsService, INotificationsReadRepository notisRead)
        {
            _notisWrite = repo;
            _stringConnection = config["ConnectionStrings:SqlServerConnection"]!;
            _notificationsHubContext = nhc;
            _userNotificationsService = notificationsService;
            _notisRead = notisRead;
        }
        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();
            var response = await _notisRead.GetUserNotificationsAsync(connection, userId);
            return response;
        }
        public async Task MarkAllNotificationsAsRead(string userId)
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();
            await _notisWrite.MarkAllNotificationsAsRead(connection, userId);
        }
        public async Task MarkSingleNotificationAsRead(int nId)
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();
            await _notisWrite.MarkSingleNotificationAsRead(connection, nId);
        }
        public async Task<SendFollowRequestResponse> SendFollowRequest(string actorId, string recipientId)
        {
            var r = new SendFollowRequestResponse();

            if (actorId == recipientId)
            {
                r.UserSendsHimselfFR = true;
                return r;
            }

            int notificationId = 0;

            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            //Tomar como sender al usuario actual!!!

            r = await _notisRead.ValidateSendFollowRequestAsync(
                connection,
                recipientId,
                actorId
            );

            if (!r.SenderExists || r.ChatExists || r.FollowRequestAlreadyExists || !r.ReceiverExists
                || r.AlreadyFriends || r.IsBlockedByOtherUser || r.IsBlockedByUser)

                return r;

            if (r.SenderIsAlreadyReceiver) // <== El usuario aceptó la solicitud de seguimiento
            {
                
                var transaction = await connection.BeginTransactionAsync();

                try
                {

                    Guid chatId = Guid.NewGuid();

                    notificationId = await _notisWrite.AddAcceptedFollowRequestDataAsync(
                        connection,
                        transaction,
                        actorId,
                        recipientId,
                        chatId
                    );

                    await transaction.CommitAsync();

                    await SendInfoByHubAsync(connection, notificationId, recipientId, actorId);

                    return r;


                } catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }

            }

            /* El usuario envía FR por primera vez por lo que el FR no existía previamente */
            notificationId = await _notisWrite.SendFollowRequestAsync(connection, actorId, recipientId);


            string? otherUserConnectionId = _userNotificationsService.GetUserConnection(recipientId.ToString());

            if (otherUserConnectionId != null)
            {
                var notification = await Extensions.GetNotificationAsync(connection, notificationId);

                await _notificationsHubContext.Clients.Client(otherUserConnectionId)
                .SendAsync("ReceivedNotification", notification);
            }

            return r;

        }

        private async Task SendInfoByHubAsync(SqlConnection connection, int notiId, string userToSend, string actorId)
        {
            string? receiverConnectionId = _userNotificationsService.GetUserConnection(userToSend);

            if (receiverConnectionId is not null)
            {
                var notification = await Extensions.GetNotificationAsync(connection, notiId);

                var popup = new MessagePopup
                {
                    ImageUrl = notification.ImageUrl,
                    NotiType = "FollowRequest",
                    Message = $"{notification.Username} has accepted your follow request.",
                    Username = notification.Username,
                    PostId = 0,
                    Status = "Accepted",
                    UserId = int.Parse(actorId)
                };

                await _notificationsHubContext.Clients.Client(receiverConnectionId)
                .SendAsync("ReceivedAnyNotification", popup);
                await _notificationsHubContext.Clients.Client(receiverConnectionId)
                .SendAsync("ReceivedNotification", notification);

            }

        }

        //public async Task<RespondToUserFollowRequestResponse> RespondToUserFollowRequest(string recipientUserId, int actorId, int notificationId, bool isAccepted)
        //{

        //    using var connection = new SqlConnection(_stringConnection);
        //    await connection.OpenAsync();

        //    var response = await _repo.ValidateRespondUserFollowRequestAsync(
        //        connection,
        //        recipientUserId,
        //        actorId.ToString()
        //    );

        //    if (!response.SenderExists || !response.ReceiverExists || response.ChatExists ||
        //        response.IsBlockedByOtherUser || response.IsBlockedByOtherUser || !response.FollowRequestExists)

        //        return response;


        //    if (isAccepted)
        //    {
        //        var transaction = await connection.BeginTransactionAsync();
        //        try
        //        {
        //            Guid chatId = Guid.NewGuid();

        //            notificationId = await _repo.SetAcceptedResponseFRDataAsync(
        //                connection,
        //                transaction,
        //                actorId.ToString(),
        //                recipientUserId,
        //                chatId
        //            );

        //            await transaction.CommitAsync();

        //            await SendAcceptedResponseByHubAsync(
        //                connection,
        //                actorId.ToString(),
        //                recipientUserId,
        //                notificationId
        //            );

        //        } catch
        //        {
        //            await transaction.RollbackAsync();
        //            throw;
        //        }

        //    } else
        //    {
        //        await connection.ExecuteAsync(
        //        FRQueries.IsFRrejected,
        //        new { actorId, recipientUserId });
        //    }

        //    return response;

        //}

        //private async Task SendAcceptedResponseByHubAsync(SqlConnection connection, string userSenderId, string userReceiverId, int notificationId)
        //{
        //    string? otherUserConnectionId = _userNotificationsService.GetUserConnection(userSenderId);

        //    if (otherUserConnectionId is not null)
        //    {
        //        var notification = await Extensions.GetNotificationAsync(connection, notificationId);

        //        await _notificationsHubContext.Clients.Client(otherUserConnectionId)
        //        .SendAsync("ReceivedAnyNotification",
        //        new MessagePopup
        //        {
        //            ImageUrl = notification.ImageUrl,
        //            NotiType = "FRAccepted",
        //            Message = $"{notification.Username} has accepted your follow request.",
        //            Username = notification.Username,
        //            PostId = 0,
        //            /*UserReceiverId vendría siendo el usuario que recibió el FR,
        //            por lo que el userSender es el interesado en saber que su FR
        //            fue aceptada y quiere ser redireccionado al que aceptó su FR*/
        //            UserId = int.Parse(userReceiverId)
        //        });

        //        await _notificationsHubContext.Clients.Client(otherUserConnectionId)
        //        .SendAsync("ReceivedNotification", notification);
        //    }
        //}

        public async Task RejectFRAsync(int recipientId, int actorId)
        {
            using var connection = new SqlConnection(_stringConnection);

            await connection.OpenAsync();

            await _notisWrite.RejectFRAsync(connection, recipientId, actorId);

        }
    }
}
