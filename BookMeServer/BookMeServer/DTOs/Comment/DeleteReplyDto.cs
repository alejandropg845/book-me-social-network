using System.ComponentModel.DataAnnotations;

namespace BookMeServer.DTOs.Comment
{
    public class DeleteReplyDto
    {
        [Required] public int CommentId { get; set; }
        [Required] public int ReplyId { get; set; }
    }
}
