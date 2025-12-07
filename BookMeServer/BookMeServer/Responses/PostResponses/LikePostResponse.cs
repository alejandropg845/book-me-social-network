namespace BookMeServer.Responses.PostResponses
{
    public class LikePostResponse
    {
        public bool UserExists { get; set; }
        public bool PostExists { get; set; }
        public bool IsBlockedByUser { get; set; }
        public bool IsBlockedByOtherUser { get; set; }
        public bool PostIsLiked { get; set; }
    }
}
