namespace BookMeServer.Models
{
    public class Post
    {
        public int Id { get; set; }
        public int PostUserId { get; set; }
        public string AuthorImageUrl { get; set; }
        public string PostImageUrl { get; set; }
        public string Username { get; set; }
        public string Description { get; set; }
        public int PostLikes { get; set; }
        public DateTime PostedDate { get; set; }
        public int CommentsNumber { get; set; }
        public bool IsLikedByUser { get; set; }
    }
}
