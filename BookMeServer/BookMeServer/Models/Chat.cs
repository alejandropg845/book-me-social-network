namespace BookMeServer.Models
{
    public class Chat
    {
        public string Id { get; set; }
        public string OtherUserId { get; set; }
        public string ImageUrl { get; set; }
        public string Username { get; set; }
        public string LastChatMessage { get; set; }
        public int LastMessageUserId { get; set; }
        public int NoReadMessages { get; set; }
        public bool IsBlockedByUser { get; set; }
        public int MessagesNumber {  get; set; }
    }
}
