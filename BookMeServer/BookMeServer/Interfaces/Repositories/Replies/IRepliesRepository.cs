using BookMeServer.Models;
using BookMeServer.Responses.CommentResponses;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace BookMeServer.Interfaces.Repositories.Replies
{
    public interface IRepliesRepository
    {
        Task<IEnumerable<CommentReply>> GetCommentRepliesAsync(SqlConnection connection, int commentId, int? number, string? userId);
        Task<ReplyToCommentResponse> ValidateReplyToCommentAsync(SqlConnection connection, IDbTransaction transaction, int commentId, string userId, int postId);
        Task<int> InsertAndGetCommentReplyAsync(SqlConnection connection, IDbTransaction transaction, int commentId, string userId, int replyingToId, string content);
        Task<int> AddCommentReplyNotificationAsync(SqlConnection connection, IDbTransaction transaction, string userId, int postId, int replyingToId);
        Task<DeleteReplyResponse> ValidateDeleteReplyAsync(SqlConnection connection, IDbTransaction transaction, int commentId, string userId, int replyId);
        Task SetReplyAsDeletedAsync(SqlConnection connection, IDbTransaction transaction, string userId, int replyId);

    }
}
