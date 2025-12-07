using System.ComponentModel.DataAnnotations;

namespace BookMeServer.DTOs.FollowRequest
{
    public class AddFollowRequestDto
    {
        [Required] public int UserSenderId { get; set; }
        [Required] public int UserReceiverId { get; set; }
    }
}
