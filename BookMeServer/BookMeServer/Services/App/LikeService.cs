using BookMeServer.DTOs.Comment;
using BookMeServer.Hubs;
using BookMeServer.Interfaces.Repositories.Likes;
using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using BookMeServer.Responses.CommentResponses;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;

namespace BookMeServer.Services.App
{
    public class LikeService : ILikeService
    {
        private readonly string _stringConnection;
        private readonly INotificationsService _hubConnectionService;
        private readonly IHubContext<NotificationsHub> _hubNotificationContext;
        private readonly ILikeRepository _repo;
        public LikeService(IConfiguration config, INotificationsService hubConnectionService, IHubContext<NotificationsHub> hubNotisContext, ILikeRepository repo)
        {
            _stringConnection = config["ConnectionStrings:SqlServerConnection"]!;
            _hubConnectionService = hubConnectionService;
            _hubNotificationContext = hubNotisContext;
            _repo = repo;
        }

        public async Task<LikeReplyResponse> LikeReplyAsync(string userId, LikeReplyDto dto)
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            var transaction = await connection.BeginTransactionAsync();

            var notificationId = 0;

            var response = new LikeReplyResponse();

            try
            {
                response = await _repo.ValidateLikeReplyAsync(
                    connection,
                    transaction,
                    dto.ReplyId,
                    userId,
                    dto.CommentId,
                    dto.PostId
                );

                if (!response.UserExists || !response.ReplyExists || !response.PostExists || !response.CommentExists) return response;


                if (!response.IsAlreadyLiked)
                {
                    notificationId = await _repo.AddNonLikedReplyDataAsync(
                        connection, transaction, dto.ReplyId,
                        userId, dto.LikingToId, dto.PostId
                    );
                }
                else
                    await _repo.RevertLikedReplyDataAsync(
                        connection, transaction, dto.ReplyId,
                        userId, dto.LikingToId
                    );

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            string? otherUserConnectionId =
                       _hubConnectionService.GetUserConnection(dto.LikingToId.ToString());


            if (otherUserConnectionId is not null && !response.IsAlreadyLiked)
            {
                if (dto.LikingToId.ToString() != userId) // <== El usuario no se da auto-like 
                {
                    var noti = await Extensions.GetNotificationAsync(connection, notificationId);

                    /* Notification para Hub */
                    await _hubNotificationContext.Clients.Client(otherUserConnectionId)
                        .SendAsync("ReceivedNotification", noti);

                    /* Notification para popup por Hub */
                    await _hubNotificationContext.Clients.Client(otherUserConnectionId)
                    .SendAsync("ReceivedAnyNotification", new MessagePopup
                    {
                        ImageUrl = noti.ImageUrl,
                        Username = noti.Username,
                        NotiType = "LC",
                        Message = $"{noti.Username} liked your reply",
                        PostId = dto.PostId,
                        UserId = 0
                    });
                }
            }

            return response;

        }
        public async Task<LikeCommentResponse> LikeCommentAsync(string userId, LikeCommentDto dto)
        {

            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            var transaction = await connection.BeginTransactionAsync();

            int notificationId = 0;
            var response = new LikeCommentResponse();
            try
            {
                response = await _repo.ValidateLikeCommentAsync(
                    connection,
                    transaction,
                    userId,
                    dto.CommentId,
                    dto.PostId
                );

                if (!response.UserExists || !response.CommentExists || !response.PostExists) return response;

                if (response.IsAlreadyLiked)

                    await _repo.RevertLikedCommentDataAsync(connection, transaction, dto.CommentId, userId, dto.AuthorIdComment);

                else

                    notificationId = await _repo.AddLikeCommentDataAsync(connection, transaction, dto.CommentId, userId, dto.AuthorIdComment, dto.PostId);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            /* Obtener id del otro usuario si se encuentra conectado */
            string? otherUserConnectionId =
                _hubConnectionService.GetUserConnection(dto.AuthorIdComment.ToString());

            //Enviar notificación de comentario likeado al autor del comentario
            if (!response.IsAlreadyLiked && otherUserConnectionId != null)
            {
                if (int.Parse(userId) != dto.AuthorIdComment) // <== El notificationId cuenta con un valor
                {
                    var notification = await Extensions.GetNotificationAsync(connection, notificationId);

                    /* Enviar notification por Hub */
                    await _hubNotificationContext.Clients.Client(otherUserConnectionId)
                        .SendAsync("ReceivedNotification", notification);

                    /* Enviar notification por popup en el client */
                    await _hubNotificationContext.Clients
                    .Client(otherUserConnectionId)
                    .SendAsync("ReceivedAnyNotification", new MessagePopup
                    {
                        ImageUrl = notification.ImageUrl,
                        Username = notification.Username,
                        NotiType = "LC",
                        Message = $"{notification.Username} has liked your comment",
                        PostId = dto.PostId,
                        UserId = 0
                    });
                }
            }

            return response;


        }
    }
}
