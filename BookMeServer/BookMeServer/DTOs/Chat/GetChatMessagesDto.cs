using BookMeServer.Models;

namespace BookMeServer.DTOs.Chat
{
    public class GetChatMessagesDto
    {
        public int OtherUserId { get; set; }
        public List<ChatMessage> ChatMessages { get; set; }
        public string ChatId { get; set; }
    }
}
