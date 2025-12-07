using BookMeServer.DTOs.Comment;
using BookMeServer.DTOs.Post;
using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookMeServer.Controllers
{
    [ApiController]
    [Route("api/like")]
    public class LikesController : ControllerBase
    {
        private readonly ILikeService _service;
        public LikesController(ILikeService service)
        {
            _service = service;
        }
        [HttpPut("likeReply")]
        [Authorize]
        public async Task<ActionResult> LikeReply([FromBody] LikeReplyDto dto)
        {

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var r = await _service.LikeReplyAsync(userId, dto);

            if (!r.ReplyExists) return NotFound(new { Message = "This comment no longer exists" });

            if (!r.CommentExists) return NotFound(new { Message = "This comment no longer exists" });

            if (!r.PostExists) return NotFound(new { Message = "The post of this comment no longer exists" });

            return Ok();

        }

        [HttpPut("likeComment")]
        [Authorize]
        public async Task<ActionResult> LikeComment([FromBody] LikeCommentDto dto)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var r = await _service.LikeCommentAsync(userId, dto);

            if (!r.UserExists) return Unauthorized(new { Message = "You have been banned from BookMe for uploading explicit content" });

            if (!r.CommentExists) return NotFound(new { Message = "This comment doesn't exist or was removed" });


            if (!r.PostExists) return NotFound(new { Message = "The post of this comment no longer exists" });

            return Ok();

        }
        
    }
}
