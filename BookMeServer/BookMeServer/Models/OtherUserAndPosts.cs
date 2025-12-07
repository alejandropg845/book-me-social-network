namespace BookMeServer.Models
{
    public class OtherUserAndPosts
    {
        public OtherUserProfile OtherUserProfile { get; set; }
        public IEnumerable<Post> OtherUserPosts { get; set; }
    }
}
