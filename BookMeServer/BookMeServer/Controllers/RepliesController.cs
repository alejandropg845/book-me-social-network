using BookMeServer.DTOs.Comment;
using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookMeServer.Controllers
{
    [ApiController]
    [Route("api/reply")]
    public class RepliesController : ControllerBase
    {
        private readonly IRepliesService _service;
        public RepliesController(IRepliesService service)
        {
            _service = service;
        }

        [HttpGet("commentReplies/{commentId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetCommentReplies([FromRoute] int commentId, [FromQuery] int number)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var commentReplies = await _service.GetCommentRepliesAsync(commentId, number, userId);
            return Ok(commentReplies);
        }
        [HttpPost("replyToComment")]
        [Authorize]
        public async Task<ActionResult> ReplyToComment([FromBody] ReplyToCommentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var r = await _service.ReplyToCommentAsync(userId, dto);

            if (!r.CommentExists) return NotFound(new { Message = "This comment no longer exists" });

            if (!r.ReplierExists) return Unauthorized();

            if (!r.PostExists) return NotFound(new { Message = "The post of this comment no longer exists" });
            
            return Ok(
                new
                {
                    Message = "Reply added successfully",
                    r.InsertedReply
                }
            );
        }
        [HttpPut("deleteReply")]
        [Authorize]
        public async Task<ActionResult> DeleteReplyAsync([FromBody] DeleteReplyDto dto)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var r = await _service.DeleteReplyAsync(userId, dto);

            if (!r.CommentExists)
                return NotFound(new { Message = "Seems like this comment no longer exists" });

            if (!r.ReplyExists) return NotFound(new { Message = "Seems like this reply no longer exists" });

            return Ok(new { Message = "Deleted successfully" });

        }

    }
}
