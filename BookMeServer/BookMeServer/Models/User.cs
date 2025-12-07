namespace BookMeServer.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string ImageUrl { get; set; }
        public string Password { get; set; }
        public string Status { get; set; }
        public string RoleName { get; set; }
    }
}
