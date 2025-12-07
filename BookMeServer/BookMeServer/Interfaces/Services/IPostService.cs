using BookMeServer.DTOs.Post;
using BookMeServer.Models;
using BookMeServer.Responses.PostResponses;

namespace BookMeServer.Interfaces.Services
{
    public interface IPostService
    {
        Task<IEnumerable<Post>> GetAllPostsAsync(string? userId);
        Task<IEnumerable<Post>> GetUserPostsAsync(string? userId, int otherUserProfileId);
        Task<Post?> GetSinglePostAsync(int postId, string userId);
        Task<AddPostResponse> AddPostAsync(string userId, AddPostDto post);
        Task<DeletePostResponse> DeletePostAsync(string userId, int postId);
        Task<LikePostResponse> LikePostAsync(int postId, string userId, int postUserId);

    }
}
