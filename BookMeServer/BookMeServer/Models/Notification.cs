namespace BookMeServer.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ActorId { get; set; }
        public string ImageUrl { get; set; }
        public string Username { get; set; }
        public bool IsRead { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
    }
}
