using BookMeServer.DTOs.Post;
using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookMeServer.Controllers
{
    [ApiController]
    [Route("api/post")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _service;

        public PostController(IPostService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Post>>> GetAllPosts()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var posts = await _service.GetAllPostsAsync(userId);
            
            return Ok(posts);
        }

        [HttpPut("likePost")]
        [Authorize]
        public async Task<ActionResult> LikePost([FromBody] LikePostDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);


            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _service.LikePostAsync(dto.PostId, userId, dto.PostUserId);

            if (!result.UserExists)
                return Unauthorized(new { Message = "You have been banned from BookMe for uploading explicit content" });

            if (!result.PostExists)
                return NotFound(new { Message = "Post no longer exists" });

            if (result.IsBlockedByUser)
                return BadRequest(new { Message = "You must unlock this user first to like its post" });

            if (result.IsBlockedByOtherUser)
                return BadRequest(new { Message = "This user has blocked you" });

            return Ok();
        }

        [HttpGet("userPosts")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Post>>> GetUserPosts()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var posts = await _service.GetUserPostsAsync(userId, int.Parse(userId));
            
            return Ok(posts);
        }

        [HttpGet("post/{postId:int}")]
        [Authorize]
        public async Task<ActionResult<Post>> GetSinglePost([FromRoute] int postId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var post = await _service.GetSinglePostAsync(postId, userId);
            if (post is null) return NotFound(new { Message = "This post no longer exists" });
            return Ok(post);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> AddPost(AddPostDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.AddPostAsync(userId, dto);

            if (!result.UserExists) return Unauthorized(new { Message = "You have been banned from BookMe for uploading explicit content" });

            if (result.IsExplicitContent)
                return BadRequest
                    (new { Message = "Please avoid uploading NSFW or offensive content." });

            return Ok(new { Message = "Post added successfully" });

        }

        [HttpDelete("{postId:int}")]
        [Authorize]
        public async Task<ActionResult> DeletePost([FromRoute] int postId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _service.DeletePostAsync(userId, postId);

            if (!result.UserExists) return Unauthorized(new { Message = "You have been banned from BookMe for uploading explicit content" });

            if(!result.PostExists) return NotFound(new { Message = "This post has been already deleted" });

            return Ok(new { Message = "Post deleted successfully" });

        }

    }
}
