using System.ComponentModel.DataAnnotations;

namespace BookMeServer.DTOs.User
{
    public class ChangeProfilePicDto
    {
        [Required] public string ProfilePicUrl { get; set; }
        [Required] public string PublicId { get; set; }
    }
}
