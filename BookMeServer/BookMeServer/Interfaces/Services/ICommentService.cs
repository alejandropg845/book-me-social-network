using BookMeServer.DTOs.Comment;
using BookMeServer.Models;
using BookMeServer.Responses.CommentResponses;

namespace BookMeServer.Interfaces.Services
{
    public interface ICommentService
    {
        Task<AddCommentResponse> AddCommentAsync(int postId, AddCommentDto dto, string userId);
        Task<DeleteCommentResponse> DeleteCommentAsync(int commentId, string userId, int postId);
        Task<IEnumerable<Comment>> GetPostCommentsAsync(int postId, int? number, string? userId);

    }
}
