using BookMeServer.DTOs.Auth;
using BookMeServer.DTOs.User;
using BookMeServer.Interfaces.Services;
using BookMeServer.Models;
using BookMeServer.Responses.UserResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookMeServer.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly ITokenService _tokenService;
        public UsersController(IUserService repo, ITokenService tokenService, IPostService postsRepo)
        {
            _userService = repo;
            _tokenService = tokenService;
            _postService = postsRepo;
        }

        [HttpGet("keepAlive")]
        public async Task<ActionResult> KeepAlive()
        {
            await _userService.KeepAliveAsync();
            return Ok();
        }

        [HttpGet("userCredentials")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> GetUserCredentials()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                return Ok(new User
                {
                    Id = 0,
                    ImageUrl = "",
                    Password = "",
                    RoleName = "",
                    Status = "",
                    Username = "guest"
                });
            }

            var userCredentials = await _userService.GetUserCredentialsForSidebarAndMyProfile(userId);
            return Ok(userCredentials);
        }


        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser(RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userService.RegisterUser(dto);

            if (result.UsernameExists)
            
                return BadRequest(new { Message = "This username is already in use" });
            
            return Ok(new
            {
                Message = "Registered successfully",
                Ok = true,
                Token = _tokenService.CreateToken(result.UserId, result.RoleName)
            });

        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginUser(LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _userService.LoginUser(dto);

            if (response.User is null) return BadRequest(new { Message = "Incorrect credentials" });

            if (response.IsDisabled) return BadRequest(new { Message = "You cannot login BookMe because your account has been banned" });

            if (!response.IsCorrect) return BadRequest(new { Message = "Incorrect credentials" });

            return Ok(new
            {
                Ok = true,
                Token = _tokenService.CreateToken(response.User.Id, response.User.RoleName),
                Message = "Logged in successfully"
            });
            
        }

        [HttpPost("newProfilePic")]
        [Authorize]
        public async Task<ActionResult> ChangeUserProfilePicAsync(ChangeProfilePicDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _userService.ChangeUserProfilePicAsync(userId, dto);

            if (!result.UserExists) return Unauthorized(new { Message = "You have been banned from BookMe for uploading explicit content" });

            if (result.IsExplicitContent)
                return BadRequest(new { Message = "Please don't upload NSFW content" });

            return Ok(new { Message = "Profile pic changed successfully" });
        }

        [HttpGet("filterUsers")]
        [Authorize]
        public async Task<ActionResult<UsersFilteringResponse>> FilterUsers([FromQuery] FilterUsersDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var users = await _userService.FilterUsersAsync(dto);
            if (users == null) return NotFound();
            return Ok(users);
        }

        [HttpGet("userAndPosts/{otherProfileId:int}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetOtherUserProfileInfo([FromRoute] int otherProfileId)
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = new OtherUserAndPosts
            {
                OtherUserProfile = await _userService.GetOtherUserCredentials(currentUserId, otherProfileId.ToString()),
                OtherUserPosts = await _postService.GetUserPostsAsync(currentUserId, otherProfileId)
            };

            return Ok(user);
        }

        [HttpGet("myUserAndPosts")]
        [Authorize]
        public async Task<ActionResult> GetMyProfileInfo()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var user = new MyUserAndPosts
            {
                User = await _userService.GetUserCredentialsForSidebarAndMyProfile(userId),
                UserPosts = await _postService.GetUserPostsAsync(userId, int.Parse(userId))
            };
            return Ok(user);
        }

        [HttpPut("blockOrUnlock/{otherUserId:int}")]
        [Authorize]
        public async Task<ActionResult> BlockOrUnlockUser([FromRoute] int otherUserId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            string takenAction = await _userService.BlockOrUnlockUserAsync(userId, otherUserId);

            return Ok(new { Message = $"You {takenAction} this user" });
        }
    }
}
