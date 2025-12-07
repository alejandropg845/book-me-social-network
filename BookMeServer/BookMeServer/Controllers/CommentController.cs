using BookMeServer.DTOs.Comment;
using BookMeServer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookMeServer.Controllers
{
    [ApiController]
    [Route("api/comment")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService  _service;
        public CommentController(ICommentService service)
        {
            _service = service;
        }

        [HttpGet("postComments/{postId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetPostComments([FromRoute] int postId, [FromQuery] int? number)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comments = await _service.GetPostCommentsAsync(postId, number, userId);
            return Ok(comments);
        }

        [HttpPost("{postId:int}")]
        [Authorize]
        public async Task<ActionResult> AddComment([FromRoute] int postId, [FromBody] AddCommentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);


            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.AddCommentAsync(postId, dto, userId);

            if (!result.PostExists) return NotFound(new { Message = "This post doesn't exist" });

            if (result.CommentingUser is null) return Unauthorized(new { Message = "You have been banned for uploading explicit content" });

            if (result.IsBlockedByUser) 
                return BadRequest(new { Message = "You have blocked this user." });

            if (result.IsBlockedByOtherUser)
                return BadRequest(new { Message = "Seems like this user has blocked you" });

            return Ok(new
                { 
                    Message = "Comment added successfully",
                    result.AddedComment
                }
            );
        }

        [HttpPut("deleteComment/{commentId:int}")]
        [Authorize]
        public async Task<ActionResult> DeleteComment([FromRoute] int commentId, [FromQuery] int postId)
        {
            
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service
                                .DeleteCommentAsync(commentId, userId, postId);

            if (!result.CommentExists) return BadRequest(new { Message = "This comment no longer exists" });

            if (!result.PostExists) return BadRequest(new { Message = "This post no longer exists" });

            return Ok(new { Message = "Deleted successfully" });
        }

        
    }
}
