using System.ComponentModel.DataAnnotations;

namespace BookMeServer.DTOs.Comment
{
    public class AddCommentDto
    {
        [Required]
        [MaxLength(500, ErrorMessage = "Max. length for comment content is 500 characters")]
        public string Content { get; set; }
        [Required] public int PostUserId { get; set; }
    }
}
