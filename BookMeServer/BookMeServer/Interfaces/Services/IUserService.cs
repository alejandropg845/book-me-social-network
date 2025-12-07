using BookMeServer.DTOs.Auth;
using BookMeServer.DTOs.User;
using BookMeServer.Models;
using BookMeServer.Responses.UserResponses;

namespace BookMeServer.Interfaces.Services
{
    public interface IUserService
    {
        Task KeepAliveAsync();
        Task<LoginUserResponse> LoginUser(LoginDto dto);
        Task<CreateUserResponse> RegisterUser(RegisterDto dto);
        Task<User> GetUserCredentialsForSidebarAndMyProfile(string userId);
        Task<ChangeProfilePicResponse> ChangeUserProfilePicAsync(string userId, ChangeProfilePicDto dto);
        Task<IEnumerable<UsersFilteringResponse>> FilterUsersAsync(FilterUsersDto dto);
        Task<OtherUserProfile> GetOtherUserCredentials(string? currentUserId, string userProfileId);
        Task<string> BlockOrUnlockUserAsync(string userId, int otherUserId);

    }
}
