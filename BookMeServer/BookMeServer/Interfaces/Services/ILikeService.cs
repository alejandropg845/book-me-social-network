using BookMeServer.DTOs.Comment;
using BookMeServer.Responses.CommentResponses;

namespace BookMeServer.Interfaces.Services
{
    public interface ILikeService
    {
        Task<LikeReplyResponse> LikeReplyAsync(string userId, LikeReplyDto dto);
        Task<LikeCommentResponse> LikeCommentAsync(string userId, LikeCommentDto dto);

    }
}
