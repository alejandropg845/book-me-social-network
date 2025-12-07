using BookMeServer.Interfaces.Services;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookMeServer.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _service;
        public AdminController(IAdminService s)
        {
            _service = s;
        }

        [HttpGet("postsAdmin")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> GetPosts()
        {
            return Ok(await _service.GetPostsAsync());
        }

        [HttpGet("usersAdmin")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> GetUsers()
        => Ok(await _service.GetUsersAsync());
        

        [HttpGet("deletedPostsAdmin")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> GetDeletedPosts()
        => Ok(await _service.GetDeletedPostsAsync());
        

        [HttpGet("disabledUsersAdmin")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> GetDisabledUsers()
        => Ok(await _service.GetDisabledUsersAsync());

        [HttpPut("deletePost/{postId:int}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DeletePost([FromRoute] int postId)
        {
            await _service.DeletePostAsync(postId);

            return Ok(new { Message = "Post deleted" });
        }

        [HttpPut("disableUser/{userId:int}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DisableUser([FromRoute] int userId)
        {
            await _service.DisableUserAsync(userId);

            return Ok(new { Message = "User disabled" });
        }
    }

}
