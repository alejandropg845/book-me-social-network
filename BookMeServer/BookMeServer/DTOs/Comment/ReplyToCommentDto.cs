using System.ComponentModel.DataAnnotations;

namespace BookMeServer.DTOs.Comment
{
    public class ReplyToCommentDto
    {
        [Required] public int CommentId { get; set; }
        [Required] public int ReplyingToId { get; set; }
        [Required] public int PostId { get; set; }
        
        [Required]
        [MaxLength(150, ErrorMessage = "Max. length for reply content is 150 characters")]
        public string Content { get; set; }
    }
}
