using System.ComponentModel.DataAnnotations;

namespace BookMeServer.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(15, ErrorMessage = "Max. characters for username is 15")]
        [MinLength(5, ErrorMessage = "Min. characters for username is 5")]
        public string Username { get; set; }

        [Required]
        [MaxLength(30, ErrorMessage = "Max. length for password is 30 characters")]
        public string Password { get; set; }
    }
}
