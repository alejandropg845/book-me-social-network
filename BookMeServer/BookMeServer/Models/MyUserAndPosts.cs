namespace BookMeServer.Models
{
    public class MyUserAndPosts
    {
        public User User { get; set; }
        public IEnumerable<Post> UserPosts { get; set; }
    }
}
