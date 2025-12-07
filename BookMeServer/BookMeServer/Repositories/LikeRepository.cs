using BookMeServer.Interfaces.Repositories.Likes;
using BookMeServer.Models;
using BookMeServer.Queries;
using BookMeServer.Responses.CommentResponses;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace BookMeServer.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        public async Task<LikeReplyResponse> ValidateLikeReplyAsync(SqlConnection connection, IDbTransaction transaction, int replyId, string userId, int commentId, int postId)
        {

            var (UserExists,
                ReplyExists,
                IsAlreadyLiked,
                CommentExists,
                PostExists) = await connection.QuerySingleAsync
                <(bool UserExists,
                bool ReplyExists,
                bool IsAlreadyLiked,
                bool CommentExists,
                bool PostExists)>(
                    CommentQueries.LikeReplyValidations,
                    new { replyId, userId, commentId, postId },
                    transaction
                );
            return new LikeReplyResponse
            {
                CommentExists = CommentExists,
                PostExists = PostExists,
                ReplyExists = ReplyExists,
                UserExists = UserExists,
                IsAlreadyLiked = IsAlreadyLiked
            };
        }
        public async Task RevertLikedReplyDataAsync(SqlConnection connection, IDbTransaction transaction, int replyId, string userId, int likingToId)
        => await connection.ExecuteAsync(LikeQueries.RevertLikedReplyDataAsync, new { replyId, userId, likingToId }, transaction);
        
        public async Task<int> AddNonLikedReplyDataAsync(SqlConnection connection, IDbTransaction transaction, int replyId, string userId, int likingToId, int postId)
        {
            int notificationId = await connection.QuerySingleOrDefaultAsync<int>(
                LikeQueries.AddNonLikedReplyData, 
                new { replyId, userId, likingToId, postId }, 
                transaction
            );

            return notificationId;
        }
        public async Task<LikeCommentResponse> ValidateLikeCommentAsync(SqlConnection connection, IDbTransaction transaction, string userId, int commentId, int postId)
        {
            var (UserExists,
                CommentExists,
                IsAlreadyLiked,
                PostExists) = await connection.QuerySingleAsync
                <(bool UserExists, bool CommentExists, bool IsAlreadyLiked, bool PostExists)>
                (
                    CommentQueries.LikeCommentValidations,
                    new { userId, commentId, postId },
                    transaction
                );

            return new LikeCommentResponse
            {
                CommentExists = CommentExists,
                PostExists = PostExists,
                UserExists = UserExists,
                IsAlreadyLiked = IsAlreadyLiked
            };
        }
        public async Task RevertLikedCommentDataAsync(SqlConnection connection, IDbTransaction transaction, int commentId, string userId, int authorIdComment)
        {
            await connection.ExecuteAsync(
                LikeQueries.RevertLikedCommentData,
                new { commentId, userId = int.Parse(userId), authorIdComment },
                transaction
            );
        }
        public async Task<int> AddLikeCommentDataAsync(SqlConnection connection, IDbTransaction transaction, int commentId, string userId, int authorIdComment, int postId)
        {
            int notificationId = await connection
            .QueryFirstOrDefaultAsync<int>(
                LikeQueries.AddLikedCommentData,
                new { commentId, userId, authorIdComment, postId },
                transaction
            );

            return notificationId;
        }
    }
}
