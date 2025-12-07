using BookMeServer.Models;
using BookMeServer.Responses.PostResponses;
using Microsoft.Data.SqlClient;

namespace BookMeServer.Interfaces.Repositories.Posts
{
    public interface IPostReadRepository
    {
        Task<IEnumerable<Post>> GetAllPostsAsync(SqlConnection connection, string? userId);
        Task<IEnumerable<Post>> GetUserPostsAsync(SqlConnection connection, string? userId, int otherUserProfileId);
        Task<Post?> GetSinglePostAsync(SqlConnection connection, int postId, string userId);
        Task<int> GetUserIdAsync(SqlConnection connection, string userId);
        Task<DeletePostResponse> ValidateDeletePostAsync(SqlConnection connection, int postId, string userId);
        Task<LikePostResponse> ValidateLikePostAsync(SqlConnection connection, string userId, int postId, int postUserId);

    }
}
