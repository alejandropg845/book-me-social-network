using System.ComponentModel.DataAnnotations;

namespace BookMeServer.DTOs.User
{
    public class FilterUsersDto
    {
        [Required] public string Username { get; set; }
    }
}
