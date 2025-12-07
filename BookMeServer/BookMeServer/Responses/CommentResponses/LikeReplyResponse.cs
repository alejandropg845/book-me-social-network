using BookMeServer.Models;

namespace BookMeServer.Responses.CommentResponses
{
    public class LikeReplyResponse
    {
        public bool UserExists { get; set; }
        public bool ReplyExists { get; set; }
        public bool IsAlreadyLiked { get; set; }
        public bool CommentExists { get; set; }
        public bool PostExists { get; set; }
    }
}
