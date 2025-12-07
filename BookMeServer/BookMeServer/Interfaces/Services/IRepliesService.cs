using BookMeServer.DTOs.Comment;
using BookMeServer.Models;
using BookMeServer.Responses.CommentResponses;

namespace BookMeServer.Interfaces.Services
{
    public interface IRepliesService
    {
        Task<IEnumerable<CommentReply>> GetCommentRepliesAsync(int commentId, int? number, string? userId);
        Task<ReplyToCommentResponse> ReplyToCommentAsync(string userId, ReplyToCommentDto dto);
        Task<DeleteReplyResponse> DeleteReplyAsync(string userId, DeleteReplyDto dto);

    }
}
