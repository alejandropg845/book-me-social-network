using BookMeServer.Interfaces.Repositories.Replies;
using BookMeServer.Models;
using BookMeServer.Queries;
using BookMeServer.Responses.CommentResponses;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace BookMeServer.Repositories
{
    public class RepliesRepository : IRepliesRepository
    {
        public async Task<IEnumerable<CommentReply>> GetCommentRepliesAsync(SqlConnection connection, int commentId, int? number, string? userId)
        {
            number ??= 0;
            string query;

            if (userId is not null)
                query = CommentQueries.GetCommentReplies;
            else
                query = CommentQueries.GetCommentRepliesAsAnonymous;

            var replies = await connection.QueryAsync<CommentReply>(
                query,
                new { commentId, number, userId }
            );

            return replies.Reverse();
            
        }
        public async Task<ReplyToCommentResponse> ValidateReplyToCommentAsync(SqlConnection connection, IDbTransaction transaction, int commentId, string userId, int postId)
        {
            var (CommentExists,
                ReplierExists,
                PostExists) = await connection.QuerySingleAsync
                <(bool CommentExists,
                bool ReplierExists,
                bool PostExists)>(
                    CommentQueries.ReplyToCommentValidations,
                    new { commentId, userId, postId },
                    transaction
                );
            return new ReplyToCommentResponse
            {
                CommentExists = CommentExists,
                PostExists = PostExists,
                ReplierExists = ReplierExists
            };
        }

        public async Task<int> InsertAndGetCommentReplyAsync(SqlConnection connection, IDbTransaction transaction, int commentId, string userId, int replyingToId, string content)
        {
            int result = await connection.QueryFirstAsync<int>(
                CommentQueries.InsertAndGetReply,
                new
                {
                    commentId,
                    userId,
                    replyingToId,
                    content
                },
                transaction
            );

            return result;
        }
        public async Task<int> AddCommentReplyNotificationAsync(SqlConnection connection, IDbTransaction transaction, string userId, int postId, int replyingToId)
        {

            int notificationId = await connection.QuerySingleAsync<int>(
                CommentQueries.AddCommentReplyNotification,
                new { postId, userId, replyingToId },
                transaction
            );

            return notificationId;

        }
        public async Task<DeleteReplyResponse> ValidateDeleteReplyAsync(SqlConnection connection, IDbTransaction transaction, int commentId, string userId, int replyId)
        {
            var (CommentExists, ReplyExists) = await connection
                .QuerySingleAsync<(bool CommentExists, bool ReplyExists)>
                (
                    CommentQueries.DeleteReplyValidations,
                    new { commentId, userId, replyId }
                );

            return new DeleteReplyResponse
            {
                CommentExists = CommentExists,
                ReplyExists = ReplyExists,
            };
        }
        public async Task SetReplyAsDeletedAsync(SqlConnection connection, IDbTransaction transaction, string userId, int replyId)
        {
            await connection.ExecuteAsync
            (
                CommentQueries.SetReplyAsDeleted,
                new { userId, replyId },
                transaction
            );
        }
    }
}
