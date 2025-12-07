using System.ComponentModel.DataAnnotations;

namespace BookMeServer.DTOs.Comment
{
    public class LikeCommentDto
    {
        [Required] public int PostId { get; set; }
        [Required] public int CommentId { get; set; }
        [Required] public int AuthorIdComment { get; set; }
    }
}
