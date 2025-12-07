using System.ComponentModel.DataAnnotations;

namespace BookMeServer.DTOs.Post
{
    public class LikePostDto
    {
        [Required] public int PostId { get; set; }
        [Required] public int PostUserId { get; set; }
    }
}
