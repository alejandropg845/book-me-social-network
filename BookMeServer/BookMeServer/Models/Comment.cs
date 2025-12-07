namespace BookMeServer.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string AuthorImage { get; set; }
        public bool IsCommentOwner { get; set; }
        public string Author { get; set; }
        public int AuthorId { get; set; }
        public string Content { get; set; }
        public DateTime CommentDate { get; set; }
        public int CommentLikes { get; set; }
        public int RepliesNumber { get; set; }
        public int PostId { get; set; }
        public bool IsLiked { get; set; }
        public bool IsDeleted { get; set; }
    }
}
