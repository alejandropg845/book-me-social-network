namespace BookMeServer.Responses.CommentResponses
{
    public class LikeCommentResponse
    {
        public bool UserExists { get; set; }
        public bool CommentExists { get; set; }
        public bool IsAlreadyLiked { get; set; }
        public bool PostExists { get; set; }
    }
}
