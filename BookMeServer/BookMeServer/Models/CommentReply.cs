namespace BookMeServer.Models
{
    public class CommentReply
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public string AuthorUsername { get; set; }
        public string ImageUrl { get; set; }
        public int ReplyLikes { get; set; }
        public int ReplyingToId { get; set; }
        public string ReplyingToUsername { get; set; }
        public DateTime RepliedAt { get; set; }
        public string Content { get; set; }
        public bool IsLikedByUser { get; set; }
        public bool IsReplyOwner { get; set; }
    }
}
