using BookMeServer.Models;

namespace BookMeServer.Responses.CommentResponses
{
    public class AddCommentResponse
    {
        public bool PostExists { get; set; }
        public string? CommentingUser { get; set; }
        public bool IsBlockedByOtherUser { get; set; }
        public bool IsBlockedByUser { get; set; }
        public Comment AddedComment { get; set; }
    }
}
