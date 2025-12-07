using Azure;
using BookMeServer.DTOs.Comment;
using BookMeServer.Hubs;
using BookMeServer.Interfaces.Repositories.Comments;
using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using BookMeServer.Queries;
using BookMeServer.Responses.CommentResponses;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Transactions;
using System.Xml.Linq;

namespace BookMeServer.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        public async Task<Comment> AddCommentAsync(SqlConnection connection, IDbTransaction transaction, string commentingUser, string content, int postId, string userId)
        {
            var inserted = await connection
            .QuerySingleOrDefaultAsync<(DateTime CommentDate, string ImageUrl, int Id)>
            (
                CommentQueries.AddAndGetCommentInfo,
                new
                {
                    Author = commentingUser,
                    content,
                    PostId = postId,
                    userId
                }, 
                transaction
            );

            var comment = new Comment
            {
                Author = commentingUser,
                Content = content,
                AuthorId = int.Parse(userId),
                AuthorImage = inserted.ImageUrl,
                CommentDate = inserted.CommentDate,
                CommentId = inserted.Id,
                IsCommentOwner = true,
                PostId = postId
            };

            return comment;
        }

        public async Task<int> AddCommentNotificationAsync(SqlConnection connection, IDbTransaction transaction, string userId, int postUserId, int postId)
        {
            int notificationId = await connection.QuerySingleAsync<int>(
                CommentQueries.AddCommentNotificationToUser,
                new { userId, postUserId, postId },
                transaction
            );

            return notificationId;
        }

        public async Task<AddCommentResponse> ValidateAddCommentAsync(SqlConnection connection, IDbTransaction transaction, int postUserId, string userId, int postId)
        {
            var (PostExists,
                IsBlockedByUser,
                IsBlockedByOtherUser,
                CommentingUser) = await connection
                .QuerySingleAsync
                <(bool PostExists, bool IsBlockedByUser,
                bool IsBlockedByOtherUser, string CommentingUser)>
                (
                    CommentQueries.AddCommentValidations,
                    new { userId, postId, postUserId },
                    transaction
                );

            return new AddCommentResponse
            {
                PostExists = PostExists,
                IsBlockedByUser = IsBlockedByUser,
                IsBlockedByOtherUser = IsBlockedByUser,
                CommentingUser = CommentingUser 
            };
        }
        
        public async Task<IEnumerable<Comment>> GetPostCommentsAsync(SqlConnection connection, int postId, int? number, string? userId)
        {

            number ??= 0;

            string query;

            if (userId is not null)
                query = CommentQueries.GetPostComments;
            else
                query = CommentQueries.GetPostCommentsAsAnonymous;

            
            var comments = await connection.QueryAsync<Comment>(
            query,
            new { postId, number, userId });

            return comments;
            
                
        }
        public async Task<DeleteCommentResponse> ValidateDeleteCommentAsync(SqlConnection connection, IDbTransaction transaction, int commentId, int postId)
        {
            var x = await connection.QuerySingleAsync
                <(bool CommentExists, bool PostExists)>
                (
                    CommentQueries.DeleteCommentValidations, 
                    new { commentId, postId },
                    transaction
                );
            return new DeleteCommentResponse
            {
                CommentExists = x.CommentExists,
                PostExists = x.PostExists
            };
        }

        public async Task SetCommentAsDeletedAsync(SqlConnection connection, IDbTransaction transaction, int commentId, string userId)
        {
            await connection.ExecuteAsync(
                CommentQueries.SetCommentAsDeleted,
                new { commentId, userId },
                transaction
            );
        }
    }
}
