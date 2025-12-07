using System.ComponentModel.DataAnnotations;

namespace BookMeServer.DTOs.Comment
{
    public class LikeReplyDto
    {
        [Required] public int CommentId { get; set; }
        [Required] public int ReplyId { get; set; }
        [Required] public int PostId { get; set; }
        [Required] public int LikingToId { get; set; }
    }
}
