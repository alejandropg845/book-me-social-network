using Azure;
using BookMeServer.DTOs.Comment;
using BookMeServer.Hubs;
using BookMeServer.Interfaces.Repositories.Comments;
using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using BookMeServer.Queries;
using BookMeServer.Responses.CommentResponses;
using Dapper;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace BookMeServer.Services.App
{
    public class CommentService : ICommentService
    {
        private readonly string _stringConnection;
        private readonly INotificationsService _hubConnectionService;
        private readonly IHubContext<NotificationsHub> _hubNotificationContext;
        private readonly ICommentRepository _repo;
        public CommentService(IConfiguration config, INotificationsService hubConnectionService, IHubContext<NotificationsHub> hubNotisContext, ICommentRepository repo)
        {
            _stringConnection = config["ConnectionStrings:SqlServerConnection"]!;
            _hubConnectionService = hubConnectionService;
            _hubNotificationContext = hubNotisContext;
            _repo = repo;
        }
        public async Task<IEnumerable<Comment>> GetPostCommentsAsync(int postId, int? number, string? userId)
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();
            var comments = await _repo.GetPostCommentsAsync(connection, postId, number, userId);

            return comments;
        }
        public async Task<AddCommentResponse> AddCommentAsync(int postId, AddCommentDto dto, string userId)
        {

            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();

            var transaction = await connection.BeginTransactionAsync();
            var response = new AddCommentResponse();
            int notificationId = 0;

            try
            {

                response = await _repo.ValidateAddCommentAsync(connection, transaction, dto.PostUserId, userId, postId);

                if (!response.PostExists) return response;
                if (response.CommentingUser is null) return response;
                if (response.IsBlockedByOtherUser) return response;
                if (response.IsBlockedByUser) return response;

                response.AddedComment = await _repo.AddCommentAsync(
                    connection,
                    transaction,
                    response.CommentingUser,
                    dto.Content,
                    postId,
                    userId
                );


                // Si el usuario se comenta, no se guarda la notificación
                if (int.Parse(userId) != dto.PostUserId)
                {
                    notificationId = await _repo.AddCommentNotificationAsync(
                        connection,
                        transaction,
                        userId,
                        dto.PostUserId,
                        postId
                    );

                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            /* Obtener connectionId del usuario al que se comentó */

            string? otherUserConnectionId = _hubConnectionService.GetUserConnection(dto.PostUserId.ToString());

            if (otherUserConnectionId != null)
            {
                if (int.Parse(userId) != dto.PostUserId)
                {
                    var n = await Extensions.GetNotificationAsync(connection, notificationId);

                    /* Enviar notification de campana por Hub */
                    await _hubNotificationContext.Clients.Client(otherUserConnectionId)
                    .SendAsync("ReceivedNotification", n);


                    await _hubNotificationContext.Clients.Client(otherUserConnectionId)
                    .SendAsync("ReceivedAnyNotification", new MessagePopup
                    {
                        ImageUrl = n.ImageUrl,
                        Username = n.Username,
                        NotiType = "C",
                        Message = $"{n.Username} has commented your post.",
                        PostId = postId,
                        UserId = 0
                    });
                }

            }

            return response;

        }
        public async Task<DeleteCommentResponse> DeleteCommentAsync(int commentId, string userId, int postId)
        {
            using var connection = new SqlConnection(_stringConnection);
            await connection.OpenAsync();
            var transaction = await connection.BeginTransactionAsync();

            try
            {
                var response = await _repo.ValidateDeleteCommentAsync(
                    connection,
                    transaction,
                    commentId,
                    postId
                );

                if (!response.PostExists || !response.CommentExists) return response;

                await _repo.SetCommentAsDeletedAsync(
                    connection,
                    transaction,
                    commentId,
                    userId
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

