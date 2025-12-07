namespace BookMeServer.Models
{
    public class MessagePopup
    {
        public string ImageUrl { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
        public string NotiType { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string? Status { get; set; }

    }
}
