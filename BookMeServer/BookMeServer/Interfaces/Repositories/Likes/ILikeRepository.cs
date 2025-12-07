using BookMeServer.Models;
using BookMeServer.Responses.CommentResponses;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace BookMeServer.Interfaces.Repositories.Likes
{
    public interface ILikeRepository
    {
        Task<LikeReplyResponse> ValidateLikeReplyAsync(SqlConnection connection, IDbTransaction transaction, int replyId, string userId, int commentId, int postId);
        Task RevertLikedReplyDataAsync(SqlConnection connection, IDbTransaction transaction, int replyId, string userId, int likingToId);
        Task<int> AddNonLikedReplyDataAsync(SqlConnection connection, IDbTransaction transaction, int replyId, string userId, int likingToId, int postId);
        Task<LikeCommentResponse> ValidateLikeCommentAsync(SqlConnection connection, IDbTransaction transaction, string userId, int commentId, int postId);
        Task RevertLikedCommentDataAsync(SqlConnection connection, IDbTransaction transaction, int commentId, string userId, int authorIdComment);
        Task<int> AddLikeCommentDataAsync(SqlConnection connection, IDbTransaction transaction, int commentId, string userId, int authorIdComment, int postId);

    }
}
