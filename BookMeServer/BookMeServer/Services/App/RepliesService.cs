using BookMeServer.DTOs.Comment;
using BookMeServer.Hubs;
using BookMeServer.Interfaces.Repositories.Replies;
using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using BookMeServer.Responses.CommentResponses;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;

namespace BookMeServer.Services.App
{
    public class RepliesService : IRepliesService
    {
        private readonly string _stringConnection;
        private readonly INotificationsService _hubConnectionService;
        private readonly IHubContext<NotificationsHub> _hubNotificationContext;
        private readonly IRepliesRepository _repo;
        public RepliesService(IConfiguration config, INotificationsService hubConnectionService, IHubContext<NotificationsHub> hubNotisContext, IRepliesRepository repo)
        {
            _stringConnection = config["ConnectionStrings:SqlServerConnection"]!;
            _hubConnectionService = hubConnectionService;
            _hubNotificationContext = hubNotisContext;
            _repo = repo;
        }

        public async Task<IEnumerable<CommentReply>> GetCommentRepliesAsync(int commentId, int? number, string? userId)
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();
            var replies = await _repo.GetCommentRepliesAsync(connection, commentId, number, userId);
            return replies;
        }
        public async Task<ReplyToCommentResponse> ReplyToCommentAsync(string userId, ReplyToCommentDto dto)
        {

            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            var transaction = await connection.BeginTransactionAsync();

            var response = new ReplyToCommentResponse();
            int notificationId = 0;
            CommentReply? insertedReply = null;
            try
            {
                response = await _repo.ValidateReplyToCommentAsync(
                    connection,
                    transaction,
                    dto.CommentId,
                    userId,
                    dto.PostId
                );

                if (!response.CommentExists || !response.PostExists || !response.ReplierExists) return response;


                int insertedCommentReplyId = await _repo.InsertAndGetCommentReplyAsync(
                    connection,
                    transaction,
                    dto.CommentId,
                    userId,
                    dto.ReplyingToId,
                    dto.Content
                );

                if (userId != dto.ReplyingToId.ToString())

                    notificationId = await _repo.AddCommentReplyNotificationAsync(
                        connection,
                        transaction,
                        userId,
                        dto.PostId,
                        dto.ReplyingToId
                    );


                await transaction.CommitAsync();

            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            string? otherUserConnectionId =
                _hubConnectionService.GetUserConnection(dto.ReplyingToId.ToString());

            if (otherUserConnectionId != null)
            {
                if (userId != dto.ReplyingToId.ToString())
                {
                    var notification = await Extensions.GetNotificationAsync(
                        connection,
                        notificationId
                    );

                    await _hubNotificationContext.Clients.Client(otherUserConnectionId)
                            .SendAsync("ReceivedNotification", notification);

                    var popup = new MessagePopup
                    {
                        ImageUrl = notification.ImageUrl,
                        Username = notification.Username,
                        NotiType = "C",
                        Message = $"{notification.Username} has replied to your comment",
                        PostId = dto.PostId,
                        UserId = 0
                    };

                    await _hubNotificationContext.Clients.Client(otherUserConnectionId)
                     .SendAsync("ReceivedAnyNotification", popup);
                }

            }
            response.InsertedReply = insertedReply!;
            return response;
        }
        public async Task<DeleteReplyResponse> DeleteReplyAsync(string userId, DeleteReplyDto dto)
        {

            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();
            var transaction = await connection.BeginTransactionAsync();


            try
            {
                var response = await _repo.ValidateDeleteReplyAsync(
                    connection,
                    transaction,
                    dto.CommentId,
                    userId,
                    dto.ReplyId
                );

                if (!response.CommentExists || !response.ReplyExists) return response;

                await _repo.SetReplyAsDeletedAsync(
                    connection,
                    transaction,
                    userId,
                    dto.ReplyId
                );

                await transaction.CommitAsync();
                return response;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }
    }
}
