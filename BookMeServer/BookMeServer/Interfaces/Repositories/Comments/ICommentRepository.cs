using BookMeServer.DTOs.Comment;
using BookMeServer.Models;
using BookMeServer.Responses.CommentResponses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace BookMeServer.Interfaces.Repositories.Comments
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetPostCommentsAsync(SqlConnection connection, int postId, int? number, string? userId);
        Task<Comment> AddCommentAsync(SqlConnection connection, IDbTransaction transaction, string commentingUser, string content, int postId, string userId);
        Task<AddCommentResponse> ValidateAddCommentAsync(SqlConnection connection, IDbTransaction transaction, int postUserId, string userId, int postId);
        Task<int> AddCommentNotificationAsync(SqlConnection connection, IDbTransaction transaction, string userId, int postUserId, int postId);
        Task SetCommentAsDeletedAsync(SqlConnection connection, IDbTransaction transaction, int commentId, string userId);
        Task<DeleteCommentResponse> ValidateDeleteCommentAsync(SqlConnection connection, IDbTransaction transaction, int commentId, int postId);
    }
}
