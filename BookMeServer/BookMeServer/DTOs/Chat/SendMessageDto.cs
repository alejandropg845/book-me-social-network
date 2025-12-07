using System.ComponentModel.DataAnnotations;

namespace BookMeServer.DTOs.Chat
{
    public class SendMessageDto
    {
        [Required] public string ChatId { get; set; }
        [Required] public string Message { get; set; }
        [Required] public string OtherUserId { get; set; }
    }

}
