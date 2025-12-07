namespace BookMeServer.Models
{
    public class ChatMessage
    {
        public DateTime SentAt {  get; set; }
        public string ChatId { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public bool IsMarkedAsRead { get; set; }

    }
}
