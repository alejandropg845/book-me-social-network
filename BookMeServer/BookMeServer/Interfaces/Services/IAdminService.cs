using BookMeServer.DTOs.Admin;

namespace BookMeServer.Interfaces.Services
{
    public interface IAdminService
    {
        Task<IEnumerable<PostsAdminDto>> GetPostsAsync();
        Task<IEnumerable<UsersAdminDto>> GetUsersAsync();
        Task<IEnumerable<DeletedPostsDto>> GetDeletedPostsAsync();
        Task<IEnumerable<string>> GetDisabledUsersAsync();
        Task DeletePostAsync(int postId);
        Task DisableUserAsync(int userId);

    }
}
